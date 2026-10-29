using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.RAG.Infrastructure;

public static class JsonDaBase
{
    public static readonly JsonSerializerOptions Opcoes = Criar();

    private static JsonSerializerOptions Criar()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opcoes.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return opcoes;
    }
}
