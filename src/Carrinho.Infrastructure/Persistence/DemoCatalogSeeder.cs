using System.Text.Json;
using Carrinho.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Carrinho.Infrastructure.Persistence;

public static class DemoCatalogSeeder
{
    public static async Task<int> SeedAsync(CarrinhoDbContext db, Stream json, CancellationToken ct = default)
    {
        var records = await JsonSerializer.DeserializeAsync<DemoProduto[]>(json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web), ct)
            ?? throw new InvalidOperationException("O catálogo de demonstração está vazio.");
        // Valida todos os registros antes de iniciar qualquer escrita.
        var produtos = records.Select(p => new Produto(p.Id, p.DescricaoProduto, p.PrecoLiquido, p.QuantidadeEstoque)).ToArray();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var inserted = 0;
        foreach (var p in produtos)
        {
            // Inserção aditiva: preserva cadastros e estoque já consumido em testes anteriores.
            inserted += await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Produto" ("ID", "DescricaoProduto", "PrecoLiquido", "QuantidadeEstoque")
                VALUES ({p.Id}, {p.DescricaoProduto}, {p.PrecoLiquido}, {p.QuantidadeEstoque})
                ON CONFLICT ("ID") DO NOTHING
                """, ct);
        }
        await transaction.CommitAsync(ct);
        return inserted;
    }

    private sealed record DemoProduto(int Id, string DescricaoProduto, decimal PrecoLiquido, int QuantidadeEstoque);
}
