using System.Text.RegularExpressions;

namespace MeuCondominio.Application.OCR.Strategies;

/// <summary>
/// Estratégia genérica de fallback.
/// Tenta extrair quaisquer referências a Bloco/Apto usando padrões amplos.
/// Usada quando nenhuma estratégia específica for detectada.
/// </summary>
public sealed class GenericOcrStrategy : IEtiquetaOcrStrategy
{
    // Padrão genérico: captura variações como "Bl. A / Ap 12", "Bloco A - Ap 12"
    private static readonly Regex _regex = new(
        @"Bl[oc\.]+\s*([A-Za-z0-9]+)[\s\-/,]+Ap[to\.]*\s*([A-Za-z0-9]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Esta estratégia sempre pode processar (é o fallback final)
    public bool PodeProcessar(string textoExtraido) => true;

    public ResultadoOcr Parse(string textoExtraido)
    {
        var match = _regex.Match(textoExtraido);
        if (!match.Success)
            return ResultadoOcr.Falha(
                "Não foi possível identificar Bloco/Apartamento automaticamente. Preenchimento manual necessário.",
                nameof(GenericOcrStrategy));

        return ResultadoOcr.ComSucesso(
            bloco:       match.Groups[1].Value.Trim().ToUpper(),
            apartamento: match.Groups[2].Value.Trim().ToUpper(),
            strategy:    nameof(GenericOcrStrategy));
    }
}
