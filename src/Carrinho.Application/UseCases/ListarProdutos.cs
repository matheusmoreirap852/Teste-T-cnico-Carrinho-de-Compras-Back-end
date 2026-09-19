using Carrinho.Application.DTOs;
using Carrinho.Application.Interfaces;

namespace Carrinho.Application.UseCases;

public sealed class ListarProdutos(IProdutoRepository repository)
{
    public async Task<IReadOnlyList<ProdutoResponse>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var produtos = await repository.ListarAsync(cancellationToken);
        return produtos.Select(p => new ProdutoResponse(
            p.Id, p.DescricaoProduto, p.PrecoLiquido, p.QuantidadeEstoque)).ToArray();
    }
}
