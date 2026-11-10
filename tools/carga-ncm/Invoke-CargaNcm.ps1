<#
.SYNOPSIS
    Carrega a tabela NCM do Classif/Siscomex na base de conhecimento da Api.RAG,
    um documento por código, via POST /v1/documentos.

.DESCRIPTION
    Script OPERACIONAL (não é projeto da solução). Alimenta o RAG apenas pela API
    REST de administração, respeitando a fronteira do Princípio 8 da constituição:
    nada acessa o PostgreSQL da Api.RAG diretamente.

    Cada registro da NCM vira um documento:
      origem          = -Origem (padrão "Classif/Siscomex")
      identificacao   = Codigo (ex.: 0101.21.00)
      versao          = ato do registro (ex.: "Res Gecex 272/2021")
      faixa           = legislacaoVigente (FR-010, maior autoridade)
      vigenciaInicial = Data_Inicio do registro
      vigenciaFinal   = Data_Fim, ou vazio quando for 31/12/9999 (intervalo aberto)
      formato         = texto
      conteudo        = "<Codigo> — <Descricao>"
      ncm             = Codigo

    A ingestão é idempotente: reenviar o mesmo conteúdo responde "ignorado" e não
    cria chunk novo (FR-007 da spec 003). Para o documento ficar CONSULTÁVEL, a
    Api.RAG precisa de OpenAI + Embedding:Dimensao/TopK/Limiar configurados; sem
    isso o documento é gravado, mas "consultavel=false". Gerar embedding tem custo.

.PARAMETER Arquivo
    Caminho do JSON da NCM. Padrão: data/ncm/ncm-classif-2026-11-10.json na raiz.

.PARAMETER BaseUrl
    URL base da Api.RAG. Ex.: http://localhost:5120 (perfil Development) ou
    http://localhost:8081 (docker do quickstart).

.PARAMETER Chave
    Chave de ingestão (cabeçalho X-Chave-Ingestao). Se vazia, lê de $env:INGESTAO_CHAVE.

.PARAMETER Origem
    Origem gravada no documento. Padrão "Classif/Siscomex".

.PARAMETER SomenteVigentes
    Só registros com Data_Fim = 31/12/9999. Padrão: ligado.

.PARAMETER SomenteOitoDigitos
    Só códigos NCM completos (8 dígitos). Padrão: desligado (ingere todos os níveis).

.PARAMETER Limite
    Máximo de registros a processar (0 = todos). Útil para teste.

.PARAMETER PausaMs
    Pausa entre requisições, em ms (vazão/custo). Padrão 0.

.PARAMETER DryRun
    Não envia nada. Monta os payloads, mostra amostra e contagem.

.EXAMPLE
    # Teste sem enviar nada:
    ./Invoke-CargaNcm.ps1 -DryRun

.EXAMPLE
    # Carga real dos 8 dígitos vigentes contra o RAG local:
    ./Invoke-CargaNcm.ps1 -BaseUrl http://localhost:5120 -Chave 'ingestao-local' -SomenteOitoDigitos

.NOTES
    Dado oficial vigente — não é base simulada. Rotacione/renove quando a NCM mudar.
#>
[CmdletBinding()]
param(
    [string]$Arquivo,
    [string]$BaseUrl = 'http://localhost:5120',
    [string]$Chave,
    [string]$Origem = 'Classif/Siscomex',
    [switch]$SomenteVigentes = $true,
    [switch]$SomenteOitoDigitos,
    [int]$Limite = 0,
    [int]$PausaMs = 0,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# --- Resolver caminhos ---------------------------------------------------------
$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $Arquivo) {
    $Arquivo = Join-Path $raiz 'data/ncm/ncm-classif-2026-11-10.json'
}
if (-not (Test-Path -LiteralPath $Arquivo)) {
    throw "Arquivo da NCM não encontrado: $Arquivo"
}

if (-not $DryRun) {
    if (-not $Chave) { $Chave = $env:INGESTAO_CHAVE }
    if (-not $Chave) {
        throw "Chave de ingestão ausente. Passe -Chave ou defina `$env:INGESTAO_CHAVE. (Em DryRun a chave não é usada.)"
    }
}

# --- Ler a tabela --------------------------------------------------------------
Write-Host "Lendo $Arquivo ..." -ForegroundColor Cyan
$json = Get-Content -LiteralPath $Arquivo -Raw -Encoding UTF8
$dados = $json | ConvertFrom-Json

$registros = @($dados.Nomenclaturas)
Write-Host ("Tabela: {0} | Ato: {1} | Registros: {2}" -f $dados.Data_Ultima_Atualizacao_NCM, $dados.Ato, $registros.Count) -ForegroundColor Cyan

# --- Helpers -------------------------------------------------------------------
function ConvertTo-IsoDate([string]$br) {
    # Entrada dd/MM/yyyy -> yyyy-MM-dd (InvariantCulture).
    $dt = [datetime]::ParseExact($br, 'dd/MM/yyyy', [System.Globalization.CultureInfo]::InvariantCulture)
    return $dt.ToString('yyyy-MM-dd')
}
function Get-Digitos([string]$codigo) { return ($codigo -replace '[^0-9]', '').Length }

$uri = ($BaseUrl.TrimEnd('/')) + '/v1/documentos'
$cabecalhos = @{ 'X-Chave-Ingestao' = $Chave }

# --- Filtrar -------------------------------------------------------------------
$fila = foreach ($r in $registros) {
    if ($SomenteVigentes -and $r.Data_Fim -ne '31/12/9999') { continue }
    if ($SomenteOitoDigitos -and (Get-Digitos $r.Codigo) -ne 8) { continue }
    $r
}
$fila = @($fila)
if ($Limite -gt 0 -and $fila.Count -gt $Limite) { $fila = $fila[0..($Limite - 1)] }
Write-Host ("A processar: {0} registros (Vigentes={1}, 8dig={2}, Limite={3})" -f $fila.Count, $SomenteVigentes, [bool]$SomenteOitoDigitos, $Limite) -ForegroundColor Cyan

# --- Processar -----------------------------------------------------------------
$cont = [ordered]@{ gravado = 0; ignorado = 0; recusado = 0; falhaTecnica = 0; erroHttp = 0; outro = 0 }
$i = 0
foreach ($r in $fila) {
    $i++
    $versao = ("{0} {1}/{2}" -f $r.Tipo_Ato_Ini, $r.Numero_Ato_Ini, $r.Ano_Ato_Ini).Trim()
    $corpo = [ordered]@{
        origem          = $Origem
        identificacao   = $r.Codigo
        versao          = $versao
        faixa           = 'legislacaoVigente'
        vigenciaInicial = ConvertTo-IsoDate $r.Data_Inicio
        formato         = 'texto'
        conteudo        = ("{0} — {1}" -f $r.Codigo, $r.Descricao)
        ncm             = $r.Codigo
    }
    if ($r.Data_Fim -ne '31/12/9999') { $corpo.vigenciaFinal = ConvertTo-IsoDate $r.Data_Fim }

    if ($DryRun) {
        if ($i -le 3) {
            Write-Host "--- amostra $i ---" -ForegroundColor Yellow
            $corpo | ConvertTo-Json -Depth 5
        }
        continue
    }

    $jsonCorpo = $corpo | ConvertTo-Json -Depth 5 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($jsonCorpo)
    try {
        $resp = Invoke-RestMethod -Method Post -Uri $uri -Headers $cabecalhos `
            -ContentType 'application/json; charset=utf-8' -Body $bytes
        $estado = "$($resp.estado)"
        if ($cont.Contains($estado)) { $cont[$estado]++ } else { $cont.outro++ }
    }
    catch {
        $cont.erroHttp++
        Write-Warning ("[{0}] {1}: {2}" -f $r.Codigo, $_.Exception.Message, $_.ErrorDetails.Message)
    }

    if ($i % 250 -eq 0) {
        Write-Host ("  {0}/{1} ... gravado={2} ignorado={3} erro={4}" -f $i, $fila.Count, $cont.gravado, $cont.ignorado, $cont.erroHttp) -ForegroundColor DarkGray
    }
    if ($PausaMs -gt 0) { Start-Sleep -Milliseconds $PausaMs }
}

# --- Resumo --------------------------------------------------------------------
Write-Host ""
if ($DryRun) {
    Write-Host ("DRY-RUN: {0} payloads montados, nada enviado." -f $fila.Count) -ForegroundColor Green
}
else {
    Write-Host "Resumo da carga:" -ForegroundColor Green
    $cont.GetEnumerator() | ForEach-Object { Write-Host ("  {0,-12} {1}" -f $_.Key, $_.Value) }
    Write-Host ("  {0,-12} {1}" -f 'total', $fila.Count)
}
