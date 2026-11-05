using Api.Agente.Application;
using Api.Agente.Domain;

namespace Api.Agente.Tests.unit;

public class MontadorResultadoSofiaTests
{
    [Fact]
    public void Divergencia_cita_somente_o_trecho_da_evidencia()
    {
        var execucao = new Execucao
        {
            Usuario = "ana",
            Empresa = "empresa-a",
            Papel = "analista",
            Finalidade = "sugerir",
            Uf = "SP",
            RegimeTributario = "lucro_real",
            TipoOperacao = "saida",
            Itens =
            [
                new ItemAnalise
                {
                    ItemId = "1",
                    TextoOriginal = "texto que não pode vazar",
                    Situacao = SituacaoItem.Divergente,
                    NcmSugerida = "73181500",
                    Evidencias =
                    [
                        new Evidencia
                        {
                            Fonte = "oficial/ncm/1",
                            Trecho = "Parafuso de aço",
                            Vigencia = new DateOnly(2026, 1, 15),
                            Ncm = "73181500"
                        }
                    ]
                }
            ]
        };

        var resultado = MontadorResultadoSofia.DaAnalise(
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222",
            new AgenteSofia { IdAgente = "agente-fiscal", Nome = "Agente Fiscal INFIS" },
            [new DocumentoAnalisadoSofia("doc-fiscal", execucao, null, null)]);

        Assert.Equal("concluido", resultado.Status);
        Assert.Equal(["Parafuso de aço"], resultado.Achados[0].Evidencias);
        Assert.DoesNotContain("não pode vazar", resultado.Analise, StringComparison.Ordinal);
        Assert.DoesNotContain("não pode vazar", resultado.Achados[0].Descricao, StringComparison.Ordinal);
    }
}
