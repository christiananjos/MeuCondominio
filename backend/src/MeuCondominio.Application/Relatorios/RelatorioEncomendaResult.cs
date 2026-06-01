namespace MeuCondominio.Application.Relatorios;

/// <summary>
/// Filtros compilados pelo Builder para serem executados no repositório.
/// </summary>
public sealed class RelatorioEncomendaFiltros
{
    public Guid      CondominioId   { get; init; }
    public DateTime? DataInicio     { get; init; }
    public DateTime? DataFim        { get; init; }
    public string?   TipoEncomenda  { get; init; }
    public string?   Status         { get; init; }
    public string?   Bloco          { get; init; }
    public string?   Apartamento    { get; init; }
}

/// <summary>
/// DTO de saída do relatório.
/// </summary>
public sealed class RelatorioEncomendaResult
{
    public int TotalPendentes    { get; init; }
    public int TotalRetiradas    { get; init; }
    public int TotalExtraviadas  { get; init; }
    public IEnumerable<EncomendaResumoItem> Itens { get; init; } = [];
}

public sealed class EncomendaResumoItem
{
    public Guid     Id               { get; init; }
    public string   Bloco            { get; init; } = string.Empty;
    public string   Apartamento      { get; init; } = string.Empty;
    public string   NomeMorador      { get; init; } = string.Empty;
    public string   TipoEncomenda    { get; init; } = string.Empty;
    public string   Status           { get; init; } = string.Empty;
    public DateTime DataRecebimento  { get; init; }
    public DateTime? DataRetirada    { get; init; }
}
