using System.Text.RegularExpressions;

namespace MeuCondominio.Application.OCR.Strategies;

/// <summary>
/// Estratégia OCR para etiquetas do Mercado Livre.
/// Padrão esperado: "Complemento: Bloco [X] Apto [Y]"
/// Exemplo real:    "Complemento: Bloco B Apto 304"
/// </summary>
public sealed class MercadoLivreOcrStrategy : IEtiquetaOcrStrategy
{
    // Compilado em tempo de inicialização para performance
    private static readonly Regex _regex = new(
        @"Complemento\s*:\s*Bloco\s+([A-Za-z0-9]+)\s+Apto\s+([A-Za-z0-9]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool PodeProcessar(string textoExtraido)
        => _regex.IsMatch(textoExtraido);

    public ResultadoOcr Parse(string textoExtraido)
    {
        var match = _regex.Match(textoExtraido);
        if (!match.Success)
            return ResultadoOcr.Falha("Padrão Mercado Livre não encontrado no texto.", nameof(MercadoLivreOcrStrategy));

        return ResultadoOcr.ComSucesso(
            bloco:       match.Groups[1].Value.Trim().ToUpper(),
            apartamento: match.Groups[2].Value.Trim().ToUpper(),
            strategy:    nameof(MercadoLivreOcrStrategy));
    }
}
