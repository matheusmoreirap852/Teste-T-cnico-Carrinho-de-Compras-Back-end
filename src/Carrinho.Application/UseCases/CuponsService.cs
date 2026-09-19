using Carrinho.Application.DTOs;
using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.Application.UseCases;

public sealed class CuponsService(ILojaRepository repository)
{
    public async Task<IReadOnlyList<CupomResponse>> ListarAsync(CancellationToken ct)
        => (await repository.CuponsAsync(ct)).Select(Map).ToArray();
    public async Task<CupomResponse> ObterAsync(int id, CancellationToken ct) => Map(await Obter(id, ct));
    public async Task<CupomResponse> CriarAsync(CriarCupomRequest r, CancellationToken ct)
    {
        if (await repository.CupomAsync(r.Id, ct) is not null) throw new ConflictException("Já existe um cupom com esse identificador.");
        var cupom = new Cupom(r.Id, r.CodigoCupom, r.PercentualDesconto);
        repository.Adicionar(cupom);
        await repository.SalvarAsync(ct);
        return Map(cupom);
    }
    public async Task<CupomResponse> AtualizarAsync(int id, AtualizarCupomRequest r, CancellationToken ct)
    {
        var cupom = await Obter(id, ct);
        cupom.Atualizar(r.CodigoCupom, r.PercentualDesconto);
        await repository.SalvarAsync(ct);
        return Map(cupom);
    }
    public async Task RemoverAsync(int id, CancellationToken ct)
    {
        repository.Remover(await Obter(id, ct));
        await repository.SalvarAsync(ct);
    }
    private async Task<Cupom> Obter(int id, CancellationToken ct) => await repository.CupomAsync(id, ct)
        ?? throw new NotFoundException("Cupom não encontrado.");
    private static CupomResponse Map(Cupom c) => new(c.Id, c.CodigoCupom, c.PercentualDesconto);
}
