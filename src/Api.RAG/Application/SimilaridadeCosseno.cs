namespace Api.RAG.Application;

public static class SimilaridadeCosseno
{
    public static double Calcular(IReadOnlyList<float> esquerda, IReadOnlyList<float> direita)
    {
        if (esquerda.Count == 0 || esquerda.Count != direita.Count)
        {
            return 0;
        }

        double produto = 0;
        double normaEsquerda = 0;
        double normaDireita = 0;
        for (var i = 0; i < esquerda.Count; i++)
        {
            produto += esquerda[i] * direita[i];
            normaEsquerda += esquerda[i] * esquerda[i];
            normaDireita += direita[i] * direita[i];
        }

        if (normaEsquerda == 0 || normaDireita == 0)
        {
            return 0;
        }

        return produto / (Math.Sqrt(normaEsquerda) * Math.Sqrt(normaDireita));
    }
}
