using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class RegistroDeLayouts
{
    public bool Possui(TipoEntrada tipo)
    {
        // A leitura de NFe (xmlNfe) extrai os itens e os NCMs declarados para conferencia.
        // Os demais formatos (planilha, pdf, laudo, ficha tecnica) seguem sem layout.
        return tipo == TipoEntrada.XmlNfe;
    }

    public bool PossuiLayoutDeItem => false;
}
