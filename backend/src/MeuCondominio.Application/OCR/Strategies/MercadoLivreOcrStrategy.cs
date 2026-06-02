using System.Text.RegularExpressions;

namespace MeuCondominio.Application.OCR.Strategies;

/// <summary>
/// Estratégia OCR para etiquetas do Mercado Livre.
/// Padrão esperado: "Complemento: Bloco [X] Apto [Y]"
/// Exemplos reais:  "Complemento: Bloco 6 Apto 3"
///                  "Complemento: Bloco B Apto 304"
/// Nota: o OCR pode inserir quebra de linha entre Bloco e Apto — \s+ cobre isso.
/// </summary>
public sealed class MercadoLivreOcrStrategy : IEtiquetaOcrStrategy
{
    // \s+ cobre espaço simples, múltiplos espaços e quebras de linha geradas pelo OCR
    private static readonly Regex _regex = new(
        @"Complemento\s*:\s*Bloco\s+([A-Za-z0-9]+)\s+Apto\s+([A-Za-z0-9]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

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
