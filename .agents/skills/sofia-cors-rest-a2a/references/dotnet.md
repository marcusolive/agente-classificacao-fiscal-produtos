# ASP.NET Core 10 - exemplo de configuracao

Em `appsettings.Development.json` (substituir por origens REAIS do navegador usado no projeto):

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "http://localhost:5173"
    ]
  }
}
```

Nao adicionar aqui `http://sofia:8080` so porque um backend contata outro backend: CORS e para browser, e URLs docker internas normalmente nao sao origens acessadas pelo navegador. Em producao, definir os dominios HTTPS reais nas configuracoes do ambiente; evitar sobrescrever outras configuracoes existentes.

Exemplo de insercao em `Program.cs` (adaptar sem duplicar os serviços ou middlewares existentes):

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

if (allowedOrigins.Length == 0)
    throw new InvalidOperationException("Configure Cors:AllowedOrigins com as origens do frontend autorizado.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("SofiaBrowser", policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization"));
});

// Preservar configuracoes existentes: AddAuthentication, AddAuthorization, A2A etc.
var app = builder.Build();

app.UseRouting();
app.UseCors("SofiaBrowser");
app.UseAuthentication(); // apenas se a autenticacao estiver configurada
app.UseAuthorization();  // apenas se a autorizacao estiver configurada

app.MapControllers();
// Preservar MapGet/MapPost/MapGroup do servidor A2A e Agent Card.
app.Run();
```

Se o agente nao atende nenhum navegador, nao habilitar CORS apenas por exigencia de integracao SOFIA->agente. Nesse caso, retirar o requisito de configuracao obrigatoria das origens e documentar que a integracao depende de HTTP, rede e autenticacao. Para politica CORS por endpoint, usar `.RequireCors("SofiaBrowser")` em rotas minimal API apropriadas ou `[EnableCors("SofiaBrowser")]` em controllers, sem combinar indiscriminadamente politicas globais e por endpoint. Ao usar `AllowCredentials`, permitir somente origens explicitamente enumeradas e avaliar CSRF e cookies antes da liberacao.

## Docker e rede

```yaml
services:
  agente-aluno:
    build: .
    environment:
      ASPNETCORE_URLS: "http://+:8080"
      Cors__AllowedOrigins__0: "http://localhost:4200"
    ports:
      - "8085:8080"
    networks:
      - agentes-network

networks:
  agentes-network:
    external: true
```

Quando SOFIA e agente compartilham a mesma rede Docker, a SOFIA chama `http://agente-aluno:8080` por HTTP interno, usando nome do servico e porta do container, nao `localhost:8085`. Essa conectividade e independente de CORS. Nao alterar politica de firewall ou permitir exposicao publica de portas sem necessidade. Proteger endpoints com autenticacao de servico (ex.: JWT/OAuth2 com audience e scopes), HTTPS onde aplicavel e limites de payload.
