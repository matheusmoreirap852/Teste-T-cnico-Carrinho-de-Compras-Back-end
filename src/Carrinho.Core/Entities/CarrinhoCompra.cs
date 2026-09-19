using Carrinho.Core.Exceptions;

namespace Carrinho.Core.Entities;

public enum StatusCarrinho { Aberto, Finalizado }

public sealed class CarrinhoCompra
{
    private readonly List<ItemCarrinho> _itens = [];
    public Guid Id { get; private set; } = Guid.NewGuid();
    public StatusCarrinho Status { get; private set; } = StatusCarrinho.Aberto;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public Guid Versao { get; private set; } = Guid.NewGuid();
    public IReadOnlyCollection<ItemCarrinho> Itens => _itens.AsReadOnly();
    public int? CupomId { get; private set; }
    public string? CodigoCupom { get; private set; }
    public decimal PercentualDesconto { get; private set; }
    public decimal Subtotal => _itens.Sum(i => i.PrecoItem);
    public decimal Desconto => decimal.Round(Subtotal * PercentualDesconto / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal Total => Subtotal - Desconto;

    public void GarantirAberto()
    {
        if (Status != StatusCarrinho.Aberto)
            throw new ConflictException("O carrinho está finalizado e não pode ser alterado.");
    }
    public void Adicionar(Produto produto, int quantidade)
    {
        GarantirAberto();
        ValidarQuantidade(quantidade);
        if (quantidade > produto.QuantidadeEstoque) throw new DomainException("Quantidade superior ao estoque disponível.");
        var item = _itens.SingleOrDefault(i => i.ProdutoId == produto.Id);
        var novaQuantidade = item is null ? 1L : (long)item.Quantidade + quantidade;
        if (novaQuantidade > produto.QuantidadeEstoque) throw new DomainException("Quantidade superior ao estoque disponível.");
        if (item is null) _itens.Add(new ItemCarrinho(Id, produto));
        else item.DefinirQuantidade((int)novaQuantidade);
        Versao = Guid.NewGuid();
    }
    public void AlterarQuantidade(int produtoId, int quantidade)
    {
        GarantirAberto();
        ValidarQuantidade(quantidade);
        var item = ObterItem(produtoId);
        if (quantidade > item.Produto.QuantidadeEstoque) throw new DomainException("Quantidade superior ao estoque disponível.");
        item.DefinirQuantidade(quantidade);
        Versao = Guid.NewGuid();
    }
    public void RemoverItem(int produtoId)
    {
        GarantirAberto();
        _itens.Remove(ObterItem(produtoId));
        Versao = Guid.NewGuid();
    }
    public void AplicarCupom(Cupom cupom)
    {
        GarantirAberto();
        CupomId = cupom.Id;
        CodigoCupom = cupom.CodigoCupom;
        PercentualDesconto = cupom.PercentualDesconto;
        Versao = Guid.NewGuid();
    }
    public void RemoverCupom()
    {
        GarantirAberto();
        CupomId = null;
        CodigoCupom = null;
        PercentualDesconto = 0;
        Versao = Guid.NewGuid();
    }
    public void Finalizar()
    {
        GarantirAberto();
        if (_itens.Count == 0) throw new DomainException("Não é possível finalizar um carrinho vazio.");
        if (_itens.Any(i => i.Quantidade > i.Produto.QuantidadeEstoque))
            throw new DomainException("Estoque insuficiente para finalizar o carrinho.");
        foreach (var item in _itens) item.Produto.BaixarEstoque(item.Quantidade);
        Status = StatusCarrinho.Finalizado;
        Versao = Guid.NewGuid();
    }
    private ItemCarrinho ObterItem(int produtoId) => _itens.SingleOrDefault(i => i.ProdutoId == produtoId)
        ?? throw new NotFoundException("Produto não encontrado no carrinho.");
    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0) throw new DomainException("A quantidade deve ser maior que zero.");
    }
}
