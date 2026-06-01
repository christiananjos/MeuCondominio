namespace MeuCondominio.Application.OCR;

/// <summary>
/// Contrato do padrão STRATEGY para processamento de etiquetas OCR.
/// Cada marketplace tem seu próprio layout de etiqueta — cada estratégia
/// encapsula o Regex e a lógica de extração específica.
/// </summary>
public interface IEtiquetaOcrStrategy
{
    /// <summary>
    /// Indica se esta estratégia é capaz de processar o texto informado.
    /// Usado pela Factory para selecionar automaticamente a estratégia correta.
    /// </summary>
    bool PodeProcessar(string textoExtraido);

    /// <summary>
    /// Extrai Bloco e Apartamento do texto OCR bruto.
    /// </summary>
    ResultadoOcr Parse(string textoExtraido);
}
