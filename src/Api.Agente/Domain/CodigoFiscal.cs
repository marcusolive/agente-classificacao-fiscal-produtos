namespace Api.Agente.Domain;

public static class CodigoFiscal
{
    public static string SomenteDigitos(string codigo) =>
        string.Concat(codigo.Where(char.IsDigit));

    public static bool MesmaNcm(string esquerda, string direita)
    {
        var uma = SomenteDigitos(esquerda);
        var outra = SomenteDigitos(direita);
        return uma.Length > 0 && uma == outra;
    }
}
