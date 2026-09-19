using Carrinho.Application.DTOs;
using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.Application.UseCases;

public sealed class CarrinhosService(ILojaRepository repository)
{
    public async Task<IReadOnlyList<CarrinhoResponse>> ListarAsync(int pagina, int tamanho, CancellationToken ct)
    {
        if (pagina < 1 || pagina > 1000000 || tamanho < 1 || tamanho > 100)
            throw new DomainException("Página deve estar entre 1 e 1000000 e tamanho entre 1 e 100.");
        return (await repository.CarrinhosAsync(pagina, tamanho, ct)).Select(CarrinhoResponse.From).ToArray();
    }
    public async Task<CarrinhoResponse> CriarAsync(CancellationToken ct)
    {
        var carrinho = new CarrinhoCompra();
        repository.Adicionar(carrinho);
        return await Salvar(carrinho, ct);
    }
    public async Task<CarrinhoResponse> ObterAsync(Guid id, CancellationToken ct) => CarrinhoResponse.From(await Obter(id, ct));
    public async Task RemoverAsync(Guid id, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.GarantirAberto();
        repository.Remover(c);
        await repository.SalvarAsync(ct);
    }
    public async Task<CarrinhoResponse> AdicionarAsync(Guid id, AdicionarItemRequest r, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.GarantirAberto();
        var p = await repository.ProdutoAsync(r.ProdutoId, ct) ?? throw new NotFoundException("Produto não encontrado.");
        c.Adicionar(p, r.Quantidade);
        return await Salvar(c, ct);
    }
    public async Task<CarrinhoResponse> QuantidadeAsync(Guid id, int produtoId, int quantidade, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.AlterarQuantidade(produtoId, quantidade);
        return await Salvar(c, ct);
    }
    public async Task<CarrinhoResponse> RemoverItemAsync(Guid id, int produtoId, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.RemoverItem(produtoId);
        return await Salvar(c, ct);
    }
    public async Task<CarrinhoResponse> AplicarCupomAsync(Guid id, string codigo, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.GarantirAberto();
        var cupom = await repository.CupomPorCodigoAsync(codigo.Trim().ToUpperInvariant(), ct)
            ?? throw new NotFoundException("Cupom inválido ou inexistente.");
        c.AplicarCupom(cupom);
        return await Salvar(c, ct);
    }
    public async Task<CarrinhoResponse> RemoverCupomAsync(Guid id, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.RemoverCupom();
        return await Salvar(c, ct);
    }
    public async Task<CarrinhoResponse> FinalizarAsync(Guid id, CancellationToken ct)
    {
        var c = await Obter(id, ct);
        c.Finalizar();
        return await Salvar(c, ct);
    }
    private async Task<CarrinhoCompra> Obter(Guid id, CancellationToken ct) => await repository.CarrinhoAsync(id, ct)
        ?? throw new NotFoundException("Carrinho não encontrado.");
    private async Task<CarrinhoResponse> Salvar(CarrinhoCompra c, CancellationToken ct)
    {
        await repository.SalvarAsync(ct);
        return CarrinhoResponse.From(c);
    }
}
