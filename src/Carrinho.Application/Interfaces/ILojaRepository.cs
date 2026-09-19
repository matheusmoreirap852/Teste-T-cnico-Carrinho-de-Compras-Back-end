using Carrinho.Core.Entities;

namespace Carrinho.Application.Interfaces;

public interface ILojaRepository
{
    Task<Produto?> ProdutoAsync(int id, CancellationToken ct);
    Task<Cupom?> CupomAsync(int id, CancellationToken ct);
    Task<Cupom?> CupomPorCodigoAsync(string codigo, CancellationToken ct);
    Task<IReadOnlyList<Cupom>> CuponsAsync(CancellationToken ct);
    Task<CarrinhoCompra?> CarrinhoAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CarrinhoCompra>> CarrinhosAsync(int pagina, int tamanho, CancellationToken ct);
    void Adicionar(Produto produto);
    void Adicionar(Cupom cupom);
    void Adicionar(CarrinhoCompra carrinho);
    void Remover(Produto produto);
    void Remover(Cupom cupom);
    void Remover(CarrinhoCompra carrinho);
    Task SalvarAsync(CancellationToken ct);
}
