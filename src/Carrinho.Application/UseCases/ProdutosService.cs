using Carrinho.Application.DTOs;
using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.Application.UseCases;

public sealed class ProdutosService(ILojaRepository repository)
{
    public async Task<ProdutoResponse> ObterAsync(int id, CancellationToken ct) => Map(await Obter(id, ct));
    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest r, CancellationToken ct)
    {
        if (await repository.ProdutoAsync(r.Id, ct) is not null) throw new ConflictException("Já existe um produto com esse identificador.");
        var produto = new Produto(r.Id, r.DescricaoProduto, r.PrecoLiquido, r.QuantidadeEstoque);
        repository.Adicionar(produto);
        await repository.SalvarAsync(ct);
        return Map(produto);
    }
    public async Task<ProdutoResponse> AtualizarAsync(int id, AtualizarProdutoRequest r, CancellationToken ct)
    {
        var produto = await Obter(id, ct);
        produto.Atualizar(r.DescricaoProduto, r.PrecoLiquido, r.QuantidadeEstoque);
        await repository.SalvarAsync(ct);
        return Map(produto);
    }
    public async Task RemoverAsync(int id, CancellationToken ct)
    {
        repository.Remover(await Obter(id, ct));
        await repository.SalvarAsync(ct);
    }
    private async Task<Produto> Obter(int id, CancellationToken ct) => await repository.ProdutoAsync(id, ct)
        ?? throw new NotFoundException("Produto não encontrado.");
    private static ProdutoResponse Map(Produto p) => new(p.Id, p.DescricaoProduto, p.PrecoLiquido, p.QuantidadeEstoque);
}
