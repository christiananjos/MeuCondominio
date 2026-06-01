using System.Text.RegularExpressions;

namespace MeuCondominio.Application.OCR.Strategies;

/// <summary>
/// Estratégia OCR para etiquetas da Amazon.
/// Padrão esperado: "Apt [Y] Blk [X]" (linha única invertida)
/// Exemplo real:    "Apt 202 Blk C"
/// </summary>
public sealed class AmazonOcrStrategy : IEtiquetaOcrStrategy
{
    private static readonly Regex _regex = new(
        @"Apt\s+([A-Za-z0-9]+)\s+Bl[ko]?\s+([A-Za-z0-9]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool PodeProcessar(string textoExtraido)
        => _regex.IsMatch(textoExtraido);

    public ResultadoOcr Parse(string textoExtraido)
    {
        var match = _regex.Match(textoExtraido);
        if (!match.Success)
            return ResultadoOcr.Falha("Padrão Amazon não encontrado no texto.", nameof(AmazonOcrStrategy));

        return ResultadoOcr.ComSucesso(
            bloco:       match.Groups[2].Value.Trim().ToUpper(),  // Bloco vem depois do Apt no padrão Amazon
            apartamento: match.Groups[1].Value.Trim().ToUpper(),
            strategy:    nameof(AmazonOcrStrategy));
    }
}
