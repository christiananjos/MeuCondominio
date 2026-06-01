using FluentAssertions;
using MeuCondominio.Application.OCR;
using MeuCondominio.Application.OCR.Strategies;

namespace MeuCondominio.Tests.Application;

public sealed class OcrStrategyTests
{
    // ──────────────────────────────────────────────────────────
    // MercadoLivreOcrStrategy
    // ──────────────────────────────────────────────────────────
    [Theory]
    [InlineData("Complemento: Bloco B Apto 304",      "B",  "304")]
    [InlineData("Complemento : Bloco  A  Apto  101",  "A",  "101")]
    [InlineData("complemento: bloco C apto 202",      "C",  "202")]
    public void MercadoLivre_TextoValido_DeveRetornarSucesso(
        string texto, string blocoEsperado, string aptoEsperado)
    {
        var strategy = new MercadoLivreOcrStrategy();

        strategy.PodeProcessar(texto).Should().BeTrue();

        var result = strategy.Parse(texto);
        result.Sucesso     .Should().BeTrue();
        result.Bloco       .Should().Be(blocoEsperado);
        result.Apartamento .Should().Be(aptoEsperado);
        result.StrategyUsed.Should().Be(nameof(MercadoLivreOcrStrategy));
    }

    [Theory]
    [InlineData("Apt 202 Blk C")]
    [InlineData("João da Silva Bloco A")]
    [InlineData("")]
    public void MercadoLivre_TextoInvalido_DevePodeProcessarFalso(string texto)
    {
        var strategy = new MercadoLivreOcrStrategy();
        strategy.PodeProcessar(texto).Should().BeFalse();
    }

    [Fact]
    public void MercadoLivre_TextoInvalido_ParseDeveRetornarFalha()
    {
        var strategy = new MercadoLivreOcrStrategy();
        var result   = strategy.Parse("texto sem padrão nenhum");

        result.Sucesso .Should().BeFalse();
        result.Mensagem.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────────────────
    // AmazonOcrStrategy
    // ──────────────────────────────────────────────────────────
    [Theory]
    [InlineData("Apt 202 Blk C",  "C",   "202")]
    [InlineData("Apt 101 Blk A",  "A",   "101")]
    [InlineData("apt 304 blk B",  "B",   "304")]
    public void Amazon_TextoValido_DeveRetornarSucesso(
        string texto, string blocoEsperado, string aptoEsperado)
    {
        var strategy = new AmazonOcrStrategy();

        strategy.PodeProcessar(texto).Should().BeTrue();

        var result = strategy.Parse(texto);
        result.Sucesso     .Should().BeTrue();
        result.Bloco       .Should().Be(blocoEsperado);
        result.Apartamento .Should().Be(aptoEsperado);
        result.StrategyUsed.Should().Be(nameof(AmazonOcrStrategy));
    }

    [Theory]
    [InlineData("Complemento: Bloco B Apto 304")]
    [InlineData("sem padrão nenhum")]
    public void Amazon_TextoInvalido_DevePodeProcessarFalso(string texto)
    {
        var strategy = new AmazonOcrStrategy();
        strategy.PodeProcessar(texto).Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────
    // GenericOcrStrategy — sempre aceita, extrai o que puder
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Generic_PodeProcessar_SempreRetornaTrue()
    {
        var strategy = new GenericOcrStrategy();
        strategy.PodeProcessar("qualquer texto").Should().BeTrue();
        strategy.PodeProcessar("").Should().BeTrue();
    }

    [Fact]
    public void Generic_Parse_DeveRetornarResultadoComStrategyCorreta()
    {
        var strategy = new GenericOcrStrategy();
        var result   = strategy.Parse("qualquer texto sem padrão específico");

        result.StrategyUsed.Should().Be(nameof(GenericOcrStrategy));
    }

    // ──────────────────────────────────────────────────────────
    // OcrStrategyFactory
    // ──────────────────────────────────────────────────────────
    [Fact]
    public void Factory_TextoMercadoLivre_DeveRetornarMercadoLivreStrategy()
    {
        var factory = CriarFactory();

        var strategy = factory.ObterEstrategia("Complemento: Bloco B Apto 304");

        strategy.Should().BeOfType<MercadoLivreOcrStrategy>();
    }

    [Fact]
    public void Factory_TextoAmazon_DeveRetornarAmazonStrategy()
    {
        var factory = CriarFactory();

        var strategy = factory.ObterEstrategia("Apt 202 Blk C");

        strategy.Should().BeOfType<AmazonOcrStrategy>();
    }

    [Fact]
    public void Factory_TextoDesconhecido_DeveRetornarGenericStrategy()
    {
        var factory = CriarFactory();

        var strategy = factory.ObterEstrategia("texto completamente desconhecido XPTO");

        strategy.Should().BeOfType<GenericOcrStrategy>();
    }

    [Fact]
    public void Factory_SemGeneric_DeveLancarExcecao()
    {
        // Sem a estratégia fallback registrada
        var factory = new OcrStrategyFactory(new IEtiquetaOcrStrategy[]
        {
            new MercadoLivreOcrStrategy(),
            new AmazonOcrStrategy()
        });

        var act = () => factory.ObterEstrategia("texto desconhecido");
        act.Should().Throw<InvalidOperationException>();
    }

    private static OcrStrategyFactory CriarFactory()
        => new(new IEtiquetaOcrStrategy[]
        {
            new MercadoLivreOcrStrategy(),
            new AmazonOcrStrategy(),
            new GenericOcrStrategy()        // fallback
        });
}
