namespace MeuCondominio.Application.Relatorios;

/// <summary>
/// Padrão BUILDER com Fluent API para construção de relatórios customizáveis.
/// Permite que o síndico componha filtros opcionais de forma encadeada.
///
/// Uso:
///   var filtros = new RelatorioEncomendaBuilder(condominioId)
///       .ComPeriodo(inicio, fim)
///       .ComStatus("Pendente")
///       .ComUnidade("B", "304")
///       .Build();
/// </summary>
public sealed class RelatorioEncomendaBuilder
{
    private readonly Guid _condominioId;
    private DateTime? _dataInicio;
    private DateTime? _dataFim;
    private string?   _tipoEncomenda;
    private string?   _status;
    private string?   _bloco;
    private string?   _apartamento;

    public RelatorioEncomendaBuilder(Guid condominioId)
    {
        if (condominioId == Guid.Empty)
            throw new ArgumentException("CondominioId é obrigatório para gerar um relatório.");

        _condominioId = condominioId;
    }

    /// <summary>Filtra por um intervalo de datas de recebimento.</summary>
    public RelatorioEncomendaBuilder ComPeriodo(DateTime inicio, DateTime fim)
    {
        if (inicio > fim) throw new ArgumentException("A data de início não pode ser posterior à data de fim.");
        _dataInicio = inicio.ToUniversalTime();
        _dataFim    = fim.ToUniversalTime();
        return this;
    }

    /// <summary>Filtra por tipo de encomenda (Caixa, Envelope, Outro).</summary>
    public RelatorioEncomendaBuilder ComTipoEncomenda(string tipo)
    {
        _tipoEncomenda = tipo.Trim();
        return this;
    }

    /// <summary>Filtra por status (Pendente, Notificado, Retirada, Extraviado).</summary>
    public RelatorioEncomendaBuilder ComStatus(string status)
    {
        _status = status.Trim();
        return this;
    }

    /// <summary>Filtra por unidade específica (bloco + apartamento).</summary>
    public RelatorioEncomendaBuilder ComUnidade(string bloco, string apartamento)
    {
        _bloco       = bloco.Trim().ToUpper();
        _apartamento = apartamento.Trim().ToUpper();
        return this;
    }

    /// <summary>Compila e retorna o objeto de filtros imutável.</summary>
    public RelatorioEncomendaFiltros Build()
        => new()
        {
            CondominioId  = _condominioId,
            DataInicio    = _dataInicio,
            DataFim       = _dataFim,
            TipoEncomenda = _tipoEncomenda,
            Status        = _status,
            Bloco         = _bloco,
            Apartamento   = _apartamento
        };
}
