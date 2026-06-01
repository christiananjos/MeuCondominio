using FluentAssertions;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;

namespace MeuCondominio.Tests.Domain;

/// <summary>
/// Testa a máquina de estados da entidade Encomenda.
/// Ciclo válido: Pendente → Notificado → Retirada
///                                     ↘ Extraviado
/// </summary>
public sealed class EncomendaStateMachineTests
{
    private static readonly Guid _condominioId = Guid.NewGuid();
    private static readonly Guid _moradorId    = Guid.NewGuid();

    // ──────────────────────────────────────────────────────────
    // Criação
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarEncomendaNoPendente()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");

        enc.Id             .Should().NotBe(Guid.Empty);
        enc.Status         .Should().Be(StatusEncomenda.Pendente);
        enc.CodigoRetirada .Should().HaveLength(4)
                           .And.MatchRegex("^[0-9]{4}$");
        enc.DataRetirada   .Should().BeNull();
    }

    [Theory]
    [InlineData("", "_moradorId")]
    public void Criar_ComTipoVazio_DeveLancarDomainException(string tipo, string _)
    {
        var act = () => Encomenda.Criar(_condominioId, _moradorId, tipo);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComCondominioIdVazio_DeveLancarDomainException()
    {
        var act = () => Encomenda.Criar(Guid.Empty, _moradorId, "Caixa");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComMoradorIdVazio_DeveLancarDomainException()
    {
        var act = () => Encomenda.Criar(_condominioId, Guid.Empty, "Caixa");
        act.Should().Throw<DomainException>();
    }

    // ──────────────────────────────────────────────────────────
    // Transição Pendente → Notificado
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Notificar_EstadoPendente_DeveTransicionarParaNotificado()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Envelope");

        enc.Notificar();

        enc.Status.Should().Be(StatusEncomenda.Notificado);
    }

    [Fact]
    public void Notificar_EstadoNotificado_DeveLancarDomainException()
    {
        var enc = CriarEncomendaNotificada();

        var act = () => enc.Notificar();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Notificar_EstadoRetirada_DeveLancarDomainException()
    {
        var enc = CriarEncomendaRetirada();

        var act = () => enc.Notificar();
        act.Should().Throw<DomainException>();
    }

    // ──────────────────────────────────────────────────────────
    // Transição Notificado → Retirada
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Retirar_EstadoNotificadoCodigoCorreto_DeveTransicionarParaRetirada()
    {
        var enc = CriarEncomendaNotificada();
        var codigo = enc.CodigoRetirada;

        enc.Retirar(codigo);

        enc.Status      .Should().Be(StatusEncomenda.Retirada);
        enc.DataRetirada.Should().NotBeNull();
        enc.DataRetirada!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Retirar_EstadoNotificadoCodigoErrado_DeveLancarDomainException()
    {
        var enc = CriarEncomendaNotificada();

        var act = () => enc.Retirar("0000");
        act.Should().Throw<DomainException>().WithMessage("*código*");
    }

    [Fact]
    public void Retirar_EstadoPendente_DeveLancarDomainException()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");

        var act = () => enc.Retirar(enc.CodigoRetirada);
        act.Should().Throw<DomainException>();
    }

    // ──────────────────────────────────────────────────────────
    // Transição Notificado → Extraviado
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Extraviar_EstadoNotificado_DeveTransicionarParaExtraviado()
    {
        var enc = CriarEncomendaNotificada();

        enc.Extraviar();

        enc.Status.Should().Be(StatusEncomenda.Extraviado);
    }

    [Fact]
    public void Extraviar_EstadoPendente_DeveTransicionarParaExtraviado()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");

        enc.Extraviar();

        enc.Status.Should().Be(StatusEncomenda.Extraviado);
    }

    [Fact]
    public void Extraviar_EstadoRetirada_DeveLancarDomainException()
    {
        var enc = CriarEncomendaRetirada();

        var act = () => enc.Extraviar();
        act.Should().Throw<DomainException>();
    }

    // ──────────────────────────────────────────────────────────
    // RecarregarEstado
    // ──────────────────────────────────────────────────────────
    [Theory]
    [InlineData(StatusEncomenda.Pendente)]
    [InlineData(StatusEncomenda.Notificado)]
    [InlineData(StatusEncomenda.Retirada)]
    [InlineData(StatusEncomenda.Extraviado)]
    public void RecarregarEstado_StatusValido_NaoDeveLancarExcecao(string status)
    {
        var enc = CriarEncomendaComStatus(status);

        var act = () => enc.RecarregarEstado();
        act.Should().NotThrow();
    }

    // ──────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────
    private static Encomenda CriarEncomendaNotificada()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");
        enc.Notificar();
        return enc;
    }

    private static Encomenda CriarEncomendaRetirada()
    {
        var enc = CriarEncomendaNotificada();
        enc.Retirar(enc.CodigoRetirada);
        return enc;
    }

    /// <summary>
    /// Cria Encomenda via reflection para testar RecarregarEstado com
    /// qualquer status sem depender de transições de estado.
    /// </summary>
    private static Encomenda CriarEncomendaComStatus(string status)
    {
        // Usa o caminho natural quando possível
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");
        if (status == StatusEncomenda.Pendente) return enc;

        enc.Notificar();
        if (status == StatusEncomenda.Notificado) return enc;

        if (status == StatusEncomenda.Retirada)
        {
            enc.Retirar(enc.CodigoRetirada);
            return enc;
        }

        enc.Extraviar();
        return enc;
    }
}
