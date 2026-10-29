using Api.Agente.Domain;

namespace Api.Agente.Application;

public sealed class RegistroDeLayouts
{
    public bool Possui(TipoEntrada tipo)
    {
        _ = tipo;
        return false;
    }

    public bool PossuiLayoutDeItem => false;
}
