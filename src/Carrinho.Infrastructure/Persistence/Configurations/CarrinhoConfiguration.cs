using Carrinho.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carrinho.Infrastructure.Persistence.Configurations;

public sealed class CarrinhoConfiguration : IEntityTypeConfiguration<CarrinhoCompra>
{
    public void Configure(EntityTypeBuilder<CarrinhoCompra> b)
    {
        b.ToTable("Carrinho");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever();
        b.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(c => c.Versao).IsConcurrencyToken();
        b.Property(c => c.CodigoCupom).HasMaxLength(50);
        b.Property(c => c.PercentualDesconto).HasPrecision(5, 2);
        b.Ignore(c => c.Subtotal);
        b.Ignore(c => c.Desconto);
        b.Ignore(c => c.Total);
        b.HasOne<Cupom>().WithMany().HasForeignKey(c => c.CupomId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(c => c.Itens).WithOne().HasForeignKey(i => i.CarrinhoId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(c => c.Itens).HasField("_itens").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ItemCarrinhoConfiguration : IEntityTypeConfiguration<ItemCarrinho>
{
    public void Configure(EntityTypeBuilder<ItemCarrinho> b)
    {
        b.ToTable("ItemCarrinho", t => t.HasCheckConstraint("CK_ItemCarrinho_Quantidade", "\"Quantidade\" > 0"));
        b.HasKey(i => new { i.CarrinhoId, i.ProdutoId });
        b.Property(i => i.PrecoLiquido).HasPrecision(18, 2);
        b.Property(i => i.DescricaoProduto).IsRequired();
        b.Ignore(i => i.PrecoItem);
        b.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
    }
}
