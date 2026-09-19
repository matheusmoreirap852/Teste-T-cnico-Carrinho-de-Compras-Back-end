using Carrinho.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carrinho.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produto", table =>
        {
            table.HasCheckConstraint("CK_Produto_PrecoLiquido", "\"PrecoLiquido\" >= 0");
            table.HasCheckConstraint("CK_Produto_QuantidadeEstoque", "\"QuantidadeEstoque\" >= 0");
        });
        builder.HasKey(p => p.Id);
        builder.Property<uint>("xmin").IsRowVersion();
        builder.Property(p => p.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(p => p.DescricaoProduto).IsRequired();
        builder.Property(p => p.PrecoLiquido).HasPrecision(18, 2);
        builder.Property(p => p.QuantidadeEstoque).IsRequired();
    }
}
