using MediatR;
using MeuCondominio.Application.DTOs;
using MeuCondominio.Application.OCR;
using MeuCondominio.Application.UseCases.Encomendas.DarBaixaEncomenda;
using MeuCondominio.Application.UseCases.Encomendas.ListarPendentes;
using MeuCondominio.Application.UseCases.Encomendas.RegistrarEncomenda;
using MeuCondominio.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeuCondominio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class EncomendasController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly OcrStrategyFactory  _ocrFactory;

    public EncomendasController(IMediator mediator, OcrStrategyFactory ocrFactory)
    {
        _mediator   = mediator;
        _ocrFactory = ocrFactory;
    }

    // ──────────────────────────────────────────────────────────
    // GET /api/encomendas/pendentes
    // ──────────────────────────────────────────────────────────
    [HttpGet("pendentes")]
    [ProducesResponseType(typeof(IEnumerable<EncomendaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPendentes(CancellationToken ct)
    {
        var condominioId = ObterCondominioId();
        var result = await _mediator.Send(new ListarPendentesQuery(condominioId), ct);
        return Ok(result);
    }

    // ──────────────────────────────────────────────────────────
    // POST /api/encomendas
    // ──────────────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarEncomendaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarEncomendaRequest request,
        CancellationToken ct)
    {
        var condominioId = ObterCondominioId();

        var command = new RegistrarEncomendaCommand(
            condominioId,
            request.MoradorId,
            request.TipoEncomenda,
            request.TextoOcrBruto);

        try
        {
            var response = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(ListarPendentes), new { }, response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // ──────────────────────────────────────────────────────────
    // POST /api/encomendas/{id}/baixa
    // ──────────────────────────────────────────────────────────
    [HttpPost("{id:guid}/baixa")]
    [ProducesResponseType(typeof(DarBaixaEncomendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DarBaixa(
        Guid   id,
        [FromBody] DarBaixaRequest request,
        CancellationToken ct)
    {
        var condominioId = ObterCondominioId();

        try
        {
            var response = await _mediator.Send(
                new DarBaixaEncomendaCommand(condominioId, id, request.CodigoInformado), ct);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // ──────────────────────────────────────────────────────────
    // POST /api/encomendas/ocr-parse
    // Endpoint auxiliar: recebe texto OCR e retorna Bloco/Apto
    // ──────────────────────────────────────────────────────────
    [HttpPost("ocr-parse")]
    [ProducesResponseType(typeof(ResultadoOcr), StatusCodes.Status200OK)]
    public IActionResult ParseOcr([FromBody] OcrParseRequest request)
    {
        var strategy = _ocrFactory.ObterEstrategia(request.TextoOcr);
        var resultado = strategy.Parse(request.TextoOcr);
        return Ok(resultado);
    }

    // ──────────────────────────────────────────────────────────
    // Extrai o condominio_id da claim do JWT do Supabase
    // ──────────────────────────────────────────────────────────
    private Guid ObterCondominioId()
    {
        var claim = User.FindFirst("condominio_id")?.Value
            ?? throw new UnauthorizedAccessException("condominio_id não encontrado no token.");
        return Guid.Parse(claim);
    }
}

// ──────────────────────────────────────────────────────────
// Request DTOs locais do Controller
// ──────────────────────────────────────────────────────────
public sealed record RegistrarEncomendaRequest(
    Guid    MoradorId,
    string  TipoEncomenda,
    string? TextoOcrBruto);

public sealed record DarBaixaRequest(string CodigoInformado);
public sealed record OcrParseRequest(string TextoOcr);
