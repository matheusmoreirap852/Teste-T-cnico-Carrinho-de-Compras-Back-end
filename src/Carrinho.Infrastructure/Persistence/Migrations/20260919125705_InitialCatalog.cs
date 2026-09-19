using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carrinho.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produto",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    DescricaoProduto = table.Column<string>(type: "text", nullable: false),
                    PrecoLiquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.ID);
                    table.CheckConstraint("CK_Produto_PrecoLiquido", "\"PrecoLiquido\" >= 0");
                    table.CheckConstraint("CK_Produto_QuantidadeEstoque", "\"QuantidadeEstoque\" >= 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produto");
        }
    }
}
