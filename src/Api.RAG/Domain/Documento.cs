using System.ComponentModel;

namespace Api.RAG.Domain;

[Description("Versão de um documento da base. Chunks desta versão não se misturam com outra.")]
public sealed class Documento
{
    [Description("Identificador gerado pelo sistema.")]
    public Guid DocumentoId { get; set; } = Guid.NewGuid();

    [Description("Origem do documento.")]
    public required string Origem { get; set; }

    [Description("Identificação do documento dentro da origem.")]
    public required string Identificacao { get; set; }

    [Description("Versão desta cópia. Conteúdo diferente exige outra versão.")]
    public required string Versao { get; set; }

    [Description("Faixa de autoridade informada na ingestão.")]
    public FaixaAutoridade Faixa { get; set; }

    [Description("Início do intervalo de vigência.")]
    public DateOnly VigenciaInicial { get; set; }

    [Description("Fim do intervalo. Vazio significa vigência aberta.")]
    public DateOnly? VigenciaFinal { get; set; }

    [Description("Formato do conteúdo original.")]
    public FormatoDocumento Formato { get; set; }

    [Description("SHA-256 do conteúdo, em hexadecimal.")]
    public required string Hash { get; set; }

    [Description("gravado, ignorado, recusado ou falha técnica.")]
    public EstadoDocumento Estado { get; set; }

    [Description("Fonte no formato origem/identificacao/versao.")]
    public string Fonte => $"{Origem}/{Identificacao}/{Versao}";

    [Description("Verdadeiro só quando a busca vetorial estiver configurada e os embeddings existirem.")]
    public bool Consultavel { get; set; }

    [Description("NCM informada na ingestão. A Api.RAG não extrai código do texto.")]
    public string? Ncm { get; set; }

    [Description("Tributos informados na ingestão, por nome.")]
    public Dictionary<string, string> Tributos { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [Description("Decisão do caso confirmado, quando a faixa for essa.")]
    public DecisaoCaso? Decisao { get; set; }

    [Description("Papel informado na confirmação do caso.")]
    public string? PapelAutoridade { get; set; }

    [Description("Data da confirmação do caso.")]
    public DateOnly? DataConfirmacao { get; set; }

    [Description("Empresa da execução de origem do caso.")]
    public string? Empresa { get; set; }

    [Description("Execução de origem do caso.")]
    public Guid? ExecucaoOrigem { get; set; }

    [Description("Trechos desta versão, na ordem de leitura.")]
    public List<Chunk> Chunks { get; set; } = [];
}

[Description("Trecho de uma única versão do documento.")]
public sealed class Chunk
{
    [Description("Identificador do trecho.")]
    public Guid ChunkId { get; set; } = Guid.NewGuid();

    [Description("Documento ao qual o trecho pertence.")]
    public Guid DocumentoId { get; set; }

    [Description("Posição do trecho na versão, começando em zero.")]
    public int Posicao { get; set; }

    [Description("Texto do trecho. É dado, não instrução.")]
    public required string Texto { get; set; }

    [Description("Página ou localização informada, quando existir.")]
    public string? Localizacao { get; set; }

    [Description("Hash do documento de origem deste trecho.")]
    public required string HashDocumento { get; set; }
}
