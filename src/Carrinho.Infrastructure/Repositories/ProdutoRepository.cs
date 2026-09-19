using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;
using Carrinho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Carrinho.Infrastructure.Repositories;

public sealed class ProdutoRepository(CarrinhoDbContext context) : IProdutoRepository
{
    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken)
        => await context.Produtos.AsNoTracking().OrderBy(p => p.Id).ToListAsync(cancellationToken);
}
