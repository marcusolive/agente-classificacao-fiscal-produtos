using System.Diagnostics;
using System.Reflection;

namespace Api.RAG.Infrastructure;

public static class NavegadorLocal
{
    public static void AgendarSwagger(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        var nome = Assembly.GetEntryAssembly()?.GetName().Name;
        if (!string.Equals(nome, "Api.RAG", StringComparison.Ordinal))
        {
            return;
        }

        app.Lifetime.ApplicationStarted.Register(() => Abrir(EnderecoSwagger(app.Urls)));
    }

    public static string EnderecoSwagger(IEnumerable<string> urls)
    {
        var bruto = urls.FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            ?? urls.FirstOrDefault()
            ?? "http://localhost:5120";
        if (!Uri.TryCreate(bruto, UriKind.Absolute, out var uri))
        {
            return "http://localhost:5120/swagger";
        }

        var host = uri.Host is "0.0.0.0" or "+" or "*" or "::" ? "localhost" : uri.Host;
        var porta = uri.IsDefaultPort ? "" : $":{uri.Port}";
        return $"{uri.Scheme}://{host}{porta}/swagger";
    }

    private static void Abrir(string endereco)
    {
        if (OperatingSystem.IsWindows() && AbrirChrome(endereco))
        {
            return;
        }

        Process.Start(new ProcessStartInfo(endereco) { UseShellExecute = true });
    }

    private static bool AbrirChrome(string endereco)
    {
        var caminhos = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        };
        var chrome = caminhos.FirstOrDefault(File.Exists);
        if (chrome is null)
        {
            return false;
        }

        Process.Start(new ProcessStartInfo(chrome, endereco) { UseShellExecute = true });
        return true;
    }
}
