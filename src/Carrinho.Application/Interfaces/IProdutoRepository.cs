using Carrinho.Core.Entities;

namespace Carrinho.Application.Interfaces;

public interface IProdutoRepository
{
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken);
}
