using FluentAssertions;
using MeuCondominio.Application.UseCases.Encomendas.DarBaixaEncomenda;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;
using MeuCondominio.Domain.Interfaces.Repositories;
using Moq;

namespace MeuCondominio.Tests.Application;

public sealed class DarBaixaEncomendaHandlerTests
{
    private readonly Mock<IEncomendaRepository> _repoMock = new();

    private static readonly Guid _condominioId = Guid.NewGuid();
    private static readonly Guid _moradorId    = Guid.NewGuid();

    private DarBaixaEncomendaHandler CriarHandler() =>
        new(_repoMock.Object);

    // ──────────────────────────────────────────────────────────
    // Caminho feliz
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_CodigoCorreto_DeveTransicionarParaRetirada()
    {
        // Cria encomenda já no estado Notificado (como viria do banco)
        var encomenda = CriarEncomendaNotificada();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(encomenda.Id, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(encomenda);

        var command = new DarBaixaEncomendaCommand(_condominioId, encomenda.Id, encomenda.CodigoRetirada);

        var response = await CriarHandler().Handle(command, CancellationToken.None);

        response.Status      .Should().Be(StatusEncomenda.Retirada);
        response.DataRetirada.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _repoMock.Verify(r => r.AtualizarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ──────────────────────────────────────────────────────────
    // Código errado → domínio lança DomainException
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_CodigoErrado_DeveLancarDomainException()
    {
        var encomenda = CriarEncomendaNotificada();

        _repoMock
            .Setup(r => r.ObterPorIdAsync(encomenda.Id, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(encomenda);

        var command = new DarBaixaEncomendaCommand(_condominioId, encomenda.Id, "0000");

        var act = async () => await CriarHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        // Não deve persistir estado inválido
        _repoMock.Verify(r => r.AtualizarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ──────────────────────────────────────────────────────────
    // Encomenda não encontrada
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_EncomendaNaoEncontrada_DeveLancarInvalidOperationException()
    {
        _repoMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Encomenda?)null);

        var command = new DarBaixaEncomendaCommand(_condominioId, Guid.NewGuid(), "1234");

        var act = async () => await CriarHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Encomenda*não encontrada*");
    }

    // ──────────────────────────────────────────────────────────
    // Estado Pendente não pode ser baixado diretamente
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_EstadoPendente_DeveLancarDomainException()
    {
        var encomenda = Encomenda.Criar(_condominioId, _moradorId, "Caixa");
        // Estado Pendente: não foi notificado ainda

        _repoMock
            .Setup(r => r.ObterPorIdAsync(encomenda.Id, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(encomenda);

        var command = new DarBaixaEncomendaCommand(_condominioId, encomenda.Id, encomenda.CodigoRetirada);

        var act = async () => await CriarHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    // ──────────────────────────────────────────────────────────
    // Helper
    // ──────────────────────────────────────────────────────────
    private static Encomenda CriarEncomendaNotificada()
    {
        var enc = Encomenda.Criar(_condominioId, _moradorId, "Caixa");
        enc.Notificar();
        // Simula recarga do banco: reset de estado
        enc.RecarregarEstado();
        return enc;
    }
}
