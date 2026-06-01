using MeuCondominio.Domain.Enums;
using MeuCondominio.Domain.Exceptions;
using MeuCondominio.Domain.States;
using System.Security.Cryptography;

namespace MeuCondominio.Domain.Entities;

/// <summary>
/// Entidade raiz de agregado. Implementa o padrão STATE para gerenciar
/// internamente o ciclo de vida: Pendente → Notificado → Retirada | Extraviado.
/// </summary>
public class Encomenda
{
    private IEncomendaState _state = null!;

    public Guid     Id               { get; private set; }
    public Guid     CondominioId     { get; private set; }
    public Guid     MoradorId        { get; private set; }
    public string   TipoEncomenda    { get; private set; } = string.Empty;
    public string   Status           { get; private set; } = string.Empty;
    public string   CodigoRetirada   { get; private set; } = string.Empty;
    public DateTime DataRecebimento  { get; private set; }
    public DateTime? DataRetirada    { get; private set; }

    // Navegação (opcional, para EF / Dapper)
    public Morador? Morador { get; private set; }

    // EF Core requer construtor sem parâmetros
    private Encomenda() { }

    // ──────────────────────────────────────────────────────────
    // Factory Method
    // ──────────────────────────────────────────────────────────
    public static Encomenda Criar(Guid condominioId, Guid moradorId, string tipoEncomenda)
    {
        if (condominioId == Guid.Empty) throw new DomainException("CondominioId inválido.");
        if (moradorId    == Guid.Empty) throw new DomainException("MoradorId inválido.");
        if (string.IsNullOrWhiteSpace(tipoEncomenda))
            throw new DomainException("Tipo de encomenda é obrigatório.");

        var encomenda = new Encomenda
        {
            Id              = Guid.NewGuid(),
            CondominioId    = condominioId,
            MoradorId       = moradorId,
            TipoEncomenda   = tipoEncomenda,
            DataRecebimento = DateTime.UtcNow,
            CodigoRetirada  = GerarCodigoSeguro()
        };
        encomenda.TransicionarPara(new PendenteState());
        return encomenda;
    }

    // ──────────────────────────────────────────────────────────
    // Reconstitui o estado ao recarregar do banco de dados
    // ──────────────────────────────────────────────────────────
    public void RecarregarEstado()
    {
        _state = Status switch
        {
            StatusEncomenda.Pendente   => new PendenteState(),
            StatusEncomenda.Notificado => new NotificadoState(),
            StatusEncomenda.Retirada   => new RetiradoState(),
            StatusEncomenda.Extraviado => new ExtravidoState(),
            _ => throw new DomainException($"Status desconhecido: '{Status}'.")
        };
    }

    // ──────────────────────────────────────────────────────────
    // Comandos de domínio (delegam ao estado atual)
    // ──────────────────────────────────────────────────────────
    public void Notificar()                           => _state.Notificar(this);
    public void Retirar(string codigoInformado)       => _state.Retirar(this, codigoInformado);
    public void Extraviar()                           => _state.Extraviar(this);

    // ──────────────────────────────────────────────────────────
    // Métodos internos chamados pelos estados (visibilidade
    // limitada ao assembly de domínio via 'internal')
    // ──────────────────────────────────────────────────────────
    internal void TransicionarPara(IEncomendaState novoEstado)
    {
        _state = novoEstado;
        Status = novoEstado.NomeStatus;
    }

    internal void DefinirDataRetirada(DateTime data) => DataRetirada = data;

    // ──────────────────────────────────────────────────────────
    // Geração criptograficamente segura de 4 dígitos
    // ──────────────────────────────────────────────────────────
    private static string GerarCodigoSeguro()
    {
        // Gera um número entre 1000-9999 usando RandomNumberGenerator
        var buffer = new byte[4];
        RandomNumberGenerator.Fill(buffer);
        var numero = (BitConverter.ToUInt32(buffer) % 9000) + 1000;
        return numero.ToString();
    }
}
