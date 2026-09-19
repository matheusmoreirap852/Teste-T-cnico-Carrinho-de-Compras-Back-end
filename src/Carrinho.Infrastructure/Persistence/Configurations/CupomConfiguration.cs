using Carrinho.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carrinho.Infrastructure.Persistence.Configurations;

public sealed class CupomConfiguration : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> b)
    {
        b.ToTable("Cupom", t => t.HasCheckConstraint("CK_Cupom_Percentual", "\"PercentualDesconto\" > 0 AND \"PercentualDesconto\" <= 100"));
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("ID").ValueGeneratedNever();
        b.Property(c => c.CodigoCupom).HasMaxLength(50).IsRequired();
        b.HasIndex(c => c.CodigoCupom).IsUnique();
        b.Property(c => c.PercentualDesconto).HasPrecision(5, 2);
        b.Property<uint>("xmin").IsRowVersion();
        b.HasData(new Cupom(1, "10OFF", 10m), new Cupom(2, "15OFF", 15m));
    }
}
