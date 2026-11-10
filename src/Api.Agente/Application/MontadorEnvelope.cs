using Api.Agente.Domain;

namespace Api.Agente.Application;

public static class MontadorEnvelope
{
    public static void Aplicar(ItemAnalise item, IntencaoAnalise intencao, RegistroDeLayouts layouts)
    {
        item.ImpactoRelevante = false;
        item.XmlItem = XmlSomenteDoItem(intencao, layouts);
    }

    private static string? XmlSomenteDoItem(IntencaoAnalise intencao, RegistroDeLayouts layouts)
    {
        if (intencao != IntencaoAnalise.SugerirXmlItem || !layouts.PossuiLayoutDeItem)
        {
            return null;
        }

        return null;
    }
}
