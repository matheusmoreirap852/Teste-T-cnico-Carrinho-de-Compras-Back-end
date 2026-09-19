using Carrinho.Application.Interfaces;
using Carrinho.Core.Entities;

namespace Carrinho.UnitTests.Support;

// Dublê dos contratos: não reproduz constraints, transações ou concorrência do PostgreSQL.
internal sealed class LojaRepositoryFake : ILojaRepository, IProdutoRepository
{
    public Dictionary<int, Produto> Produtos { get; } = [];
    public Dictionary<int, Cupom> Cupons { get; } = [];
    public Dictionary<Guid, CarrinhoCompra> Carrinhos { get; } = [];
    public List<object> Adicionados { get; } = [];
    public List<object> Removidos { get; } = [];
    public int Salvamentos { get; private set; }
    public int Consultas { get; private set; }
    public Exception? FalhaAoSalvar { get; set; }
    public CancellationToken UltimoToken { get; private set; }
    public string? CodigoConsultado { get; private set; }
    public (int Pagina, int Tamanho)? Paginacao { get; private set; }

    private Task<T> Responder<T>(T value, CancellationToken ct)
    {
        UltimoToken = ct;
        ct.ThrowIfCancellationRequested();
        Consultas++;
        return Task.FromResult(value);
    }
    public Task<Produto?> ProdutoAsync(int id, CancellationToken ct) => Responder(Produtos.GetValueOrDefault(id), ct);
    public Task<Cupom?> CupomAsync(int id, CancellationToken ct) => Responder(Cupons.GetValueOrDefault(id), ct);
    public Task<CarrinhoCompra?> CarrinhoAsync(Guid id, CancellationToken ct) => Responder(Carrinhos.GetValueOrDefault(id), ct);
    public Task<Cupom?> CupomPorCodigoAsync(string codigo, CancellationToken ct)
    {
        CodigoConsultado = codigo;
        return Responder(Cupons.Values.SingleOrDefault(c => c.CodigoCupom == codigo), ct);
    }
    public Task<IReadOnlyList<Cupom>> CuponsAsync(CancellationToken ct) => Responder<IReadOnlyList<Cupom>>(Cupons.Values.ToArray(), ct);
    public Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct) => Responder<IReadOnlyList<Produto>>(Produtos.Values.ToArray(), ct);
    public Task<IReadOnlyList<CarrinhoCompra>> CarrinhosAsync(int pagina, int tamanho, CancellationToken ct)
    {
        Paginacao = (pagina, tamanho);
        return Responder<IReadOnlyList<CarrinhoCompra>>(Carrinhos.Values.Skip((pagina - 1) * tamanho).Take(tamanho).ToArray(), ct);
    }
    public void Adicionar(Produto p) => Adicionados.Add(p);
    public void Adicionar(Cupom c) => Adicionados.Add(c);
    public void Adicionar(CarrinhoCompra c) => Adicionados.Add(c);
    public void Remover(Produto p) => Removidos.Add(p);
    public void Remover(Cupom c) => Removidos.Add(c);
    public void Remover(CarrinhoCompra c) => Removidos.Add(c);
    public Task SalvarAsync(CancellationToken ct)
    {
        UltimoToken = ct;
        ct.ThrowIfCancellationRequested();
        Salvamentos++;
        return FalhaAoSalvar is null ? Task.CompletedTask : Task.FromException(FalhaAoSalvar);
    }
}
