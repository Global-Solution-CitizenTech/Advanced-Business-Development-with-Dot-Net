namespace ZelaFix.Domain.Common;

/// <summary>A prioridade não é gravada no banco: deriva da quantidade de apoios da comunidade.</summary>
public static class PoliticaPrioridade
{
    public const int LimiteMedia = 5;
    public const int LimiteAlta = 15;
    public const int LimiteCritica = 25;

    public static string Calcular(int apoios) => apoios switch
    {
        >= LimiteCritica => "Crítica",
        >= LimiteAlta => "Alta",
        >= LimiteMedia => "Média",
        _ => "Baixa"
    };
}