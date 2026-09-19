using Carrinho.Core.Entities;

namespace Carrinho.Application.DTOs;

public sealed record CupomResponse(int Id, string CodigoCupom, decimal PercentualDesconto);
public sealed record ItemCarrinhoResponse(int ProdutoId, string DescricaoProduto, int Quantidade, decimal PrecoLiquido, int QuantidadeEstoque, decimal PrecoItem);
public sealed record CarrinhoResponse(Guid Id, string Status, DateTime CriadoEm, IReadOnlyList<ItemCarrinhoResponse> Itens,
    CupomResponse? Cupom, decimal Subtotal, decimal Desconto, decimal Total)
{
    public static CarrinhoResponse From(CarrinhoCompra c) => new(c.Id, c.Status.ToString(), c.CriadoEm,
        c.Itens.OrderBy(i => i.ProdutoId).Select(i => new ItemCarrinhoResponse(i.ProdutoId, i.DescricaoProduto,
            i.Quantidade, i.PrecoLiquido, i.Produto.QuantidadeEstoque, i.PrecoItem)).ToArray(),
        c.CupomId is int id ? new CupomResponse(id, c.CodigoCupom!, c.PercentualDesconto) : null,
        c.Subtotal, c.Desconto, c.Total);
}
