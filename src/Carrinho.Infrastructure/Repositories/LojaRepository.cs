using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;
using Carrinho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Carrinho.Infrastructure.Repositories;

public sealed class LojaRepository(CarrinhoDbContext db) : ILojaRepository
{
    public Task<Produto?> ProdutoAsync(int id, CancellationToken ct) => db.Produtos.SingleOrDefaultAsync(p => p.Id == id, ct);
    public Task<Cupom?> CupomAsync(int id, CancellationToken ct) => db.Cupons.SingleOrDefaultAsync(c => c.Id == id, ct);
    public Task<Cupom?> CupomPorCodigoAsync(string codigo, CancellationToken ct) => db.Cupons.SingleOrDefaultAsync(c => c.CodigoCupom == codigo, ct);
    public async Task<IReadOnlyList<Cupom>> CuponsAsync(CancellationToken ct) => await db.Cupons.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);
    private IQueryable<CarrinhoCompra> CarrinhosComItens => db.Carrinhos.Include(c => c.Itens).ThenInclude(i => i.Produto);
    public Task<CarrinhoCompra?> CarrinhoAsync(Guid id, CancellationToken ct) => CarrinhosComItens.SingleOrDefaultAsync(c => c.Id == id, ct);
    public async Task<IReadOnlyList<CarrinhoCompra>> CarrinhosAsync(int pagina, int tamanho, CancellationToken ct)
        => await CarrinhosComItens.AsNoTracking().OrderByDescending(c => c.CriadoEm).ThenBy(c => c.Id)
            .Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(ct);
    public void Adicionar(Produto p) => db.Produtos.Add(p);
    public void Adicionar(Cupom c) => db.Cupons.Add(c);
    public void Adicionar(CarrinhoCompra c) => db.Carrinhos.Add(c);
    public void Remover(Produto p) => db.Produtos.Remove(p);
    public void Remover(Cupom c) => db.Cupons.Remove(c);
    public void Remover(CarrinhoCompra c) => db.Carrinhos.Remove(c);
    public async Task SalvarAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Os dados foram alterados por outra operação. Consulte novamente e tente de novo.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Já existe um registro com esse identificador ou código.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new ConflictException("O registro está vinculado a um carrinho ou foi removido por outra operação.");
        }
    }
}
