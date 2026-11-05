FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Packages.props ./
COPY src/Api.Agente/Api.Agente.csproj src/Api.Agente/
RUN dotnet restore src/Api.Agente/Api.Agente.csproj
COPY src/Api.Agente/ src/Api.Agente/
RUN dotnet publish src/Api.Agente/Api.Agente.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.Agente.dll"]
