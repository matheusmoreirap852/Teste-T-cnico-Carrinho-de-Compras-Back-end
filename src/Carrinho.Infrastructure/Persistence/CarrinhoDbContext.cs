using Carrinho.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Carrinho.Infrastructure.Persistence;

public sealed class CarrinhoDbContext(DbContextOptions<CarrinhoDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cupom> Cupons => Set<Cupom>();
    public DbSet<CarrinhoCompra> Carrinhos => Set<CarrinhoCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarrinhoDbContext).Assembly);
}
