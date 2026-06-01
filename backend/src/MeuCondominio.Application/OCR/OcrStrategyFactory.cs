using MeuCondominio.Application.OCR.Strategies;

namespace MeuCondominio.Application.OCR;

/// <summary>
/// FACTORY que seleciona a estratégia OCR correta para o texto extraído.
/// Aplica o princípio Open/Closed: novos marketplaces são adicionados como
/// novas classes injetadas via DI — sem alterar esta fábrica.
/// </summary>
public sealed class OcrStrategyFactory
{
    private readonly IEnumerable<IEtiquetaOcrStrategy> _strategies;

    public OcrStrategyFactory(IEnumerable<IEtiquetaOcrStrategy> strategies)
    {
        _strategies = strategies;
    }

    /// <summary>
    /// Retorna a primeira estratégia capaz de processar o texto.
    /// A GenericOcrStrategy deve ser registrada por último (fallback).
    /// </summary>
    public IEtiquetaOcrStrategy ObterEstrategia(string textoExtraido)
    {
        foreach (var strategy in _strategies)
        {
            if (strategy is GenericOcrStrategy) continue; // Pula o fallback na primeira passagem
            if (strategy.PodeProcessar(textoExtraido))
                return strategy;
        }

        // Retorna o fallback genérico
        return _strategies.OfType<GenericOcrStrategy>().Single();
    }
}
