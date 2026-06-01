namespace MeuCondominio.Application.OCR;

/// <summary>
/// DTO com o resultado da extração OCR de uma etiqueta.
/// </summary>
public sealed class ResultadoOcr
{
    public bool    Sucesso      { get; init; }
    public string? Bloco        { get; init; }
    public string? Apartamento  { get; init; }
    public string? Mensagem     { get; init; }
    public string? StrategyUsed { get; init; }

    public static ResultadoOcr Falha(string motivo, string strategy)
        => new() { Sucesso = false, Mensagem = motivo, StrategyUsed = strategy };

    public static ResultadoOcr ComSucesso(string bloco, string apartamento, string strategy)
        => new() { Sucesso = true, Bloco = bloco, Apartamento = apartamento, StrategyUsed = strategy };
}
