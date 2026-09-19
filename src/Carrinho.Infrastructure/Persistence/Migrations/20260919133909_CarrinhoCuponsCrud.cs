using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Carrinho.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CarrinhoCuponsCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Produto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "Cupom",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    CodigoCupom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupom", x => x.ID);
                    table.CheckConstraint("CK_Cupom_Percentual", "\"PercentualDesconto\" > 0 AND \"PercentualDesconto\" <= 100");
                });

            migrationBuilder.CreateTable(
                name: "Carrinho",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Versao = table.Column<Guid>(type: "uuid", nullable: false),
                    CupomId = table.Column<int>(type: "integer", nullable: true),
                    CodigoCupom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrinho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carrinho_Cupom_CupomId",
                        column: x => x.CupomId,
                        principalTable: "Cupom",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemCarrinho",
                columns: table => new
                {
                    CarrinhoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    DescricaoProduto = table.Column<string>(type: "text", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoLiquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCarrinho", x => new { x.CarrinhoId, x.ProdutoId });
                    table.CheckConstraint("CK_ItemCarrinho_Quantidade", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_ItemCarrinho_Carrinho_CarrinhoId",
                        column: x => x.CarrinhoId,
                        principalTable: "Carrinho",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemCarrinho_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cupom",
                columns: new[] { "ID", "CodigoCupom", "PercentualDesconto" },
                values: new object[,]
                {
                    { 1, "10OFF", 10m },
                    { 2, "15OFF", 15m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrinho_CupomId",
                table: "Carrinho",
                column: "CupomId");

            migrationBuilder.CreateIndex(
                name: "IX_Cupom_CodigoCupom",
                table: "Cupom",
                column: "CodigoCupom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCarrinho_ProdutoId",
                table: "ItemCarrinho",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemCarrinho");

            migrationBuilder.DropTable(
                name: "Carrinho");

            migrationBuilder.DropTable(
                name: "Cupom");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Produto");
        }
    }
}
