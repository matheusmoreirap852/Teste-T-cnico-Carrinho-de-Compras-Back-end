namespace Carrinho.Application.DTOs;

public sealed record ProdutoResponse(
    int Id, string DescricaoProduto, decimal PrecoLiquido, int QuantidadeEstoque);
