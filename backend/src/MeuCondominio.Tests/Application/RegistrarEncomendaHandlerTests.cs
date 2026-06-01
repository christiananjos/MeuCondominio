using FluentAssertions;
using MeuCondominio.Application.UseCases.Encomendas.RegistrarEncomenda;
using MeuCondominio.Domain.Entities;
using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Interfaces;
using MeuCondominio.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeuCondominio.Tests.Application;

public sealed class RegistrarEncomendaHandlerTests
{
    private readonly Mock<IEncomendaRepository> _encomendaRepoMock = new();
    private readonly Mock<IMoradorRepository>   _moradorRepoMock   = new();
    private readonly Mock<IWhatsAppService>     _whatsAppMock      = new();
    private readonly Mock<ILogger<RegistrarEncomendaHandler>> _loggerMock = new();

    private static readonly Guid _condominioId = Guid.NewGuid();
    private static readonly Guid _moradorId    = Guid.NewGuid();

    private RegistrarEncomendaHandler CriarHandler() =>
        new(_encomendaRepoMock.Object,
            _moradorRepoMock.Object,
            _whatsAppMock.Object,
            _loggerMock.Object);

    private static Morador MoradorFake() =>
        Morador.Criar(_condominioId, "B", "304", "João Silva", "5511999990000");

    // ──────────────────────────────────────────────────────────
    // Caminho feliz
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_MoradorExistente_DeveRegistrarEncomendaEEnviarWhatsApp()
    {
        // Arrange
        var morador = MoradorFake();
        _moradorRepoMock
            .Setup(r => r.ObterPorIdAsync(_moradorId, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(morador);
        _whatsAppMock
            .Setup(w => w.EnviarNotificacaoEncomendaAsync(
                _condominioId, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RegistrarEncomendaCommand(_condominioId, _moradorId, "Caixa", null);

        // Act
        var response = await CriarHandler().Handle(command, CancellationToken.None);

        // Assert
        response.EncomendaId   .Should().NotBe(Guid.Empty);
        response.CodigoRetirada.Should().MatchRegex("^[0-9]{4}$");
        response.WhatsAppEnviado.Should().BeTrue();

        _encomendaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()), Times.Once);
        _encomendaRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhatsAppFalha_DeveRetornarEnviado_False_ESeguirSemExcecao()
    {
        var morador = MoradorFake();
        _moradorRepoMock
            .Setup(r => r.ObterPorIdAsync(_moradorId, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(morador);
        _whatsAppMock
            .Setup(w => w.EnviarNotificacaoEncomendaAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new RegistrarEncomendaCommand(_condominioId, _moradorId, "Envelope", null);

        var response = await CriarHandler().Handle(command, CancellationToken.None);

        response.WhatsAppEnviado.Should().BeFalse();
        // A encomenda deve ter sido persistida mesmo assim
        _encomendaRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ──────────────────────────────────────────────────────────
    // Morador não encontrado
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_MoradorNaoEncontrado_DeveLancarInvalidOperationException()
    {
        _moradorRepoMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Morador?)null);

        var command = new RegistrarEncomendaCommand(_condominioId, _moradorId, "Caixa", null);

        var act = async () => await CriarHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Morador*não encontrado*");
    }

    // ──────────────────────────────────────────────────────────
    // Status pós-handle
    // ──────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_Sucesso_EncomendaDeveFicarComStatusNotificado()
    {
        var morador = MoradorFake();
        _moradorRepoMock
            .Setup(r => r.ObterPorIdAsync(_moradorId, _condominioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(morador);
        _whatsAppMock
            .Setup(w => w.EnviarNotificacaoEncomendaAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Encomenda? encomendasalva = null;
        _encomendaRepoMock
            .Setup(r => r.AtualizarAsync(It.IsAny<Encomenda>(), It.IsAny<CancellationToken>()))
            .Callback<Encomenda, CancellationToken>((e, _) => encomendasalva = e);

        var command = new RegistrarEncomendaCommand(_condominioId, _moradorId, "Caixa", null);
        await CriarHandler().Handle(command, CancellationToken.None);

        encomendasalva.Should().NotBeNull();
        encomendasalva!.Status.Should().Be(StatusEncomenda.Notificado);
    }
}
