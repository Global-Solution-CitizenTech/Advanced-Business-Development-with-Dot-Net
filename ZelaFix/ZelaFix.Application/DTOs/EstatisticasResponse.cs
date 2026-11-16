namespace ZelaFix.Application.DTOs;

public sealed record EstatisticasResponse(
    long Total,
    long Abertos,
    long EmAndamento,
    long Resolvidos,
    long Rejeitados,
    long TotalApoios);
