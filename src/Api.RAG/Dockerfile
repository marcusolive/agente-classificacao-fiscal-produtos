FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Packages.props ./
COPY src/Api.RAG/Api.RAG.csproj src/Api.RAG/
RUN dotnet restore src/Api.RAG/Api.RAG.csproj
COPY src/Api.RAG/ src/Api.RAG/
RUN dotnet publish src/Api.RAG/Api.RAG.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.RAG.dll"]
