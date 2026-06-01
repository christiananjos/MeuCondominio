using FluentAssertions;
using MeuCondominio.Application.Relatorios;

namespace MeuCondominio.Tests.Application;

public sealed class RelatorioEncomendaBuilderTests
{
    private static readonly Guid _condominioId = Guid.NewGuid();

    [Fact]
    public void Build_SemFiltros_DeveRetornarSomenteCondominioId()
    {
        var filtros = new RelatorioEncomendaBuilder(_condominioId).Build();

        filtros.CondominioId .Should().Be(_condominioId);
        filtros.DataInicio   .Should().BeNull();
        filtros.DataFim      .Should().BeNull();
        filtros.Status       .Should().BeNull();
        filtros.TipoEncomenda.Should().BeNull();
        filtros.Bloco        .Should().BeNull();
        filtros.Apartamento  .Should().BeNull();
    }

    [Fact]
    public void ComPeriodo_DatasValidas_DevePreencherFiltros()
    {
        var inicio = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim    = new DateTime(2024, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        var filtros = new RelatorioEncomendaBuilder(_condominioId)
            .ComPeriodo(inicio, fim)
            .Build();

        filtros.DataInicio.Should().Be(inicio);
        filtros.DataFim   .Should().Be(fim);
    }

    [Fact]
    public void ComPeriodo_InicioMaiorQueFim_DeveLancarArgumentException()
    {
        var inicio = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var act = () => new RelatorioEncomendaBuilder(_condominioId).ComPeriodo(inicio, fim);
        act.Should().Throw<ArgumentException>().WithMessage("*início*");
    }

    [Fact]
    public void ComStatus_DevePreencherFiltroStatus()
    {
        var filtros = new RelatorioEncomendaBuilder(_condominioId)
            .ComStatus("Pendente")
            .Build();

        filtros.Status.Should().Be("Pendente");
    }

    [Fact]
    public void ComTipoEncomenda_DevePreencherFiltro()
    {
        var filtros = new RelatorioEncomendaBuilder(_condominioId)
            .ComTipoEncomenda("Caixa")
            .Build();

        filtros.TipoEncomenda.Should().Be("Caixa");
    }

    [Fact]
    public void ComUnidade_DeveNormalizarParaMaiusculo()
    {
        var filtros = new RelatorioEncomendaBuilder(_condominioId)
            .ComUnidade("b", "304")
            .Build();

        filtros.Bloco      .Should().Be("B");
        filtros.Apartamento.Should().Be("304");
    }

    [Fact]
    public void FluentChain_TodosFiltros_DeveConstruirCorretamente()
    {
        var inicio = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim    = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var filtros = new RelatorioEncomendaBuilder(_condominioId)
            .ComPeriodo(inicio, fim)
            .ComStatus("Notificado")
            .ComTipoEncomenda("Envelope")
            .ComUnidade("A", "101")
            .Build();

        filtros.CondominioId .Should().Be(_condominioId);
        filtros.DataInicio   .Should().Be(inicio);
        filtros.DataFim      .Should().Be(fim);
        filtros.Status       .Should().Be("Notificado");
        filtros.TipoEncomenda.Should().Be("Envelope");
        filtros.Bloco        .Should().Be("A");
        filtros.Apartamento  .Should().Be("101");
    }

    [Fact]
    public void Construtor_CondominioIdVazio_DeveLancarArgumentException()
    {
        var act = () => new RelatorioEncomendaBuilder(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
