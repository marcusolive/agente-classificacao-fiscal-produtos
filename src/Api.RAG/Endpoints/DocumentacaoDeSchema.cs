using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.RAG.Endpoints;

public static class DocumentacaoDeSchema
{
    public static Task Transformar(OpenApiSchema schema, OpenApiSchemaTransformerContext contexto, CancellationToken ct)
    {
        var descricao = Descricao(contexto.JsonPropertyInfo) ?? Descricao(contexto.JsonTypeInfo);
        if (!string.IsNullOrWhiteSpace(descricao))
        {
            schema.Description = descricao;
        }

        return Task.CompletedTask;
    }

    private static string? Descricao(JsonPropertyInfo? propriedade) =>
        Texto(propriedade?.AttributeProvider as MemberInfo);

    private static string? Descricao(JsonTypeInfo tipo) => Texto(tipo.Type);

    private static string? Texto(MemberInfo? membro) =>
        membro?.GetCustomAttribute<DescriptionAttribute>()?.Description;
}
