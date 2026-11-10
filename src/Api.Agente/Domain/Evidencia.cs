using System.ComponentModel;

namespace Api.Agente.Domain;

[Description("Trecho recuperado e usado como evidência. Não é instrução do sistema.")]
public sealed class Evidencia
{
    [Description("Nome da fonte consultada.")]
    public required string Fonte { get; init; }

    [Description("Faixa de autoridade usada para ordenar a evidência.")]
    public FaixaAutoridade FaixaAutoridade { get; init; }

    [Description("Trecho citado. Sem trecho, a evidência não vence.")]
    public string? Trecho { get; init; }

    [Description("Onde o trecho foi localizado na fonte.")]
    public string? Localizacao { get; init; }

    [Description("Vigência da evidência.")]
    public DateOnly Vigencia { get; init; }

    [Description("NCM presente nesta evidência.")]
    public string? Ncm { get; init; }

    [Description("Tratamentos presentes nesta evidência, por tributo.")]
    public IReadOnlyDictionary<string, string> Tributos { get; init; } = new Dictionary<string, string>();
}

[Description("Valor de um tributo na sugestão.")]
public sealed class ValorTratamento
{
    [Description("sustentado quando veio da evidência vencedora; limitacao quando não pôde ser sustentado.")]
    public SituacaoTratamento Situacao { get; init; }

    [Description("Valor sustentado. Nulo quando há limitação.")]
    public string? Valor { get; init; }

    [Description("Motivo da limitação, quando o valor não foi sustentado.")]
    public string? Motivo { get; init; }
}

[Description("Registro de um achado da execução.")]
public sealed class Achado
{
    [Description("Classe do achado.")]
    public ClasseAchado Classe { get; init; }

    [Description("erro ou aviso.")]
    public SeveridadeAchado Severidade { get; init; }

    [Description("Descrição objetiva do achado.")]
    public required string Descricao { get; init; }
}

[Description("Atributos lidos do texto do produto.")]
public sealed class AtributosProduto
{
    [Description("Função do produto. Essencial para classificar.")]
    public string? Funcao { get; init; }

    [Description("Material informado no texto.")]
    public string? Material { get; init; }

    [Description("Composição do produto. Essencial para classificar.")]
    public string? Composicao { get; init; }

    [Description("Uso informado no texto.")]
    public string? Uso { get; init; }

    [Description("Apresentação do produto. Essencial para classificar.")]
    public string? Apresentacao { get; init; }

    [Description("Termos técnicos encontrados no texto.")]
    public string? TermosTecnicos { get; init; }
}

[Description("Versão de uma base citada na execução.")]
public sealed class VersaoBase
{
    [Description("Nome da base.")]
    public required string Base { get; init; }

    [Description("Versão registrada da base.")]
    public required string Versao { get; init; }
}
