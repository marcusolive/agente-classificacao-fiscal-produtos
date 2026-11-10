using Api.RAG.Infrastructure;

namespace Api.RAG.Tests.unit;

public class NavegadorLocalTests
{
    [Fact]
    public void Endereco_usa_a_porta_local_e_o_caminho_do_swagger()
    {
        var endereco = NavegadorLocal.EnderecoSwagger(["http://localhost:5120"]);

        Assert.Equal("http://localhost:5120/swagger", endereco);
    }

    [Fact]
    public void Endereco_troca_o_curinga_por_localhost()
    {
        var endereco = NavegadorLocal.EnderecoSwagger(["http://0.0.0.0:5120"]);

        Assert.Equal("http://localhost:5120/swagger", endereco);
    }
}
