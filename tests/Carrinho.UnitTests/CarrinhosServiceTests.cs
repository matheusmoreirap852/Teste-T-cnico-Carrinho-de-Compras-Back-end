using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;
using Carrinho.UnitTests.Support;

namespace Carrinho.UnitTests;

public sealed class CarrinhosServiceTests
{
    private readonly LojaRepositoryFake repo = new();
    private readonly CarrinhoCompra carrinho = new();
    private readonly Produto produto = new(1, "Café", 19.99m, 10);
    private CarrinhosService Service => new(repo);

    public CarrinhosServiceTests()
    {
        repo.Carrinhos[carrinho.Id] = carrinho;
        repo.Produtos[1] = produto;
        repo.Cupons[1] = new Cupom(1, "10OFF", 10);
        repo.Cupons[2] = new Cupom(2, "15OFF", 15);
    }

    [Fact]
    public async Task CriarRetornaCarrinhoAbertoVazioEPersisteUmaVez()
    {
        var result = await Service.CriarAsync(default);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Aberto", result.Status);
        Assert.Empty(result.Itens);
        Assert.Null(result.Cupom);
        Assert.Equal(0m, result.Total);
        Assert.Equal(result.Id, Assert.IsType<CarrinhoCompra>(Assert.Single(repo.Adicionados)).Id);
        Assert.Equal(1, repo.Salvamentos);
    }

    [Fact]
    public async Task ConsultarRetornaPrecoEstoqueETotaisSemSalvar()
    {
        carrinho.Adicionar(produto, 1);
        carrinho.AplicarCupom(repo.Cupons[1]);
        var result = await Service.ObterAsync(carrinho.Id, default);
        var item = Assert.Single(result.Itens);
        Assert.Equal(19.99m, item.PrecoLiquido);
        Assert.Equal(10, item.QuantidadeEstoque);
        Assert.Equal(17.99m, result.Total);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Theory]
    [InlineData("obter")]
    [InlineData("remover")]
    [InlineData("adicionar")]
    [InlineData("quantidade")]
    [InlineData("removerItem")]
    [InlineData("cupom")]
    [InlineData("removerCupom")]
    [InlineData("checkout")]
    public async Task CarrinhoInexistenteNaoSalva(string operacao)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Executar(operacao, Guid.NewGuid()));
        Assert.Equal(0, repo.Salvamentos);
        Assert.Empty(repo.Removidos);
    }

    [Theory]
    [InlineData("remover")]
    [InlineData("adicionar")]
    [InlineData("quantidade")]
    [InlineData("removerItem")]
    [InlineData("cupom")]
    [InlineData("removerCupom")]
    [InlineData("checkout")]
    public async Task CarrinhoFinalizadoRejeitaTodaMutacaoSemSalvar(string operacao)
    {
        carrinho.Adicionar(produto, 1);
        carrinho.Finalizar();
        await Assert.ThrowsAsync<ConflictException>(() => Executar(operacao, carrinho.Id));
        Assert.Equal(0, repo.Salvamentos);
        Assert.Empty(repo.Removidos);
        Assert.Equal(9, produto.QuantidadeEstoque);
    }

    private Task Executar(string operacao, Guid id) => operacao switch
    {
        "obter" => Service.ObterAsync(id, default),
        "remover" => Service.RemoverAsync(id, default),
        "adicionar" => Service.AdicionarAsync(id, new(1, 1), default),
        "quantidade" => Service.QuantidadeAsync(id, 1, 2, default),
        "removerItem" => Service.RemoverItemAsync(id, 1, default),
        "cupom" => Service.AplicarCupomAsync(id, "10OFF", default),
        "removerCupom" => Service.RemoverCupomAsync(id, default),
        "checkout" => Service.FinalizarAsync(id, default),
        _ => throw new ArgumentOutOfRangeException(nameof(operacao))
    };

    [Fact]
    public async Task ProdutoInexistenteNaoAdicionaItem()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Service.AdicionarAsync(carrinho.Id, new(999, 1), default));
        Assert.Empty(carrinho.Itens);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task QuantidadeInvalidaNaoSalva(int quantidade)
    {
        await Assert.ThrowsAsync<DomainException>(() => Service.AdicionarAsync(carrinho.Id, new(1, quantidade), default));
        Assert.Empty(carrinho.Itens);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task FluxoCompletoComDadosSimuladosRecalculaResposta()
    {
        await Service.AdicionarAsync(carrinho.Id, new(1, 5), default);
        var sum = await Service.AdicionarAsync(carrinho.Id, new(1, 2), default);
        Assert.Equal(3, Assert.Single(sum.Itens).Quantidade);
        var discount = await Service.AplicarCupomAsync(carrinho.Id, "  10off  ", default);
        Assert.Equal("10OFF", repo.CodigoConsultado);
        Assert.Equal(53.97m, discount.Total);
        var changed = await Service.QuantidadeAsync(carrinho.Id, 1, 2, default);
        Assert.Equal(35.98m, changed.Total);
        var swapped = await Service.AplicarCupomAsync(carrinho.Id, "15OFF", default);
        Assert.Equal(33.98m, swapped.Total);
        Assert.Equal(2, swapped.Cupom!.Id);
        var withoutCoupon = await Service.RemoverCupomAsync(carrinho.Id, default);
        Assert.Null(withoutCoupon.Cupom);
        Assert.Equal(39.98m, withoutCoupon.Total);
        var empty = await Service.RemoverItemAsync(carrinho.Id, 1, default);
        Assert.Empty(empty.Itens);
        Assert.Equal(0m, empty.Total);
        Assert.Equal(7, repo.Salvamentos);
    }

    [Fact]
    public async Task CupomInexistentePreservaAnteriorSemSalvar()
    {
        carrinho.AplicarCupom(repo.Cupons[1]);
        await Assert.ThrowsAsync<NotFoundException>(() => Service.AplicarCupomAsync(carrinho.Id, "NAOEXISTE", default));
        Assert.Equal("10OFF", carrinho.CodigoCupom);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Theory]
    [InlineData("quantidade")]
    [InlineData("removerItem")]
    public async Task ItemInexistenteNaoSalva(string operacao)
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Executar(operacao, carrinho.Id));
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task RemoverAbertoSolicitaExclusaoEPersiste()
    {
        await Service.RemoverAsync(carrinho.Id, default);
        Assert.Same(carrinho, Assert.Single(repo.Removidos));
        Assert.Equal(1, repo.Salvamentos);
    }

    [Fact]
    public async Task CheckoutRetornaEstoqueAtualizadoEStatusFinalizado()
    {
        carrinho.Adicionar(produto, 1);
        carrinho.AlterarQuantidade(1, 3);
        var result = await Service.FinalizarAsync(carrinho.Id, default);
        Assert.Equal("Finalizado", result.Status);
        Assert.Equal(7, Assert.Single(result.Itens).QuantidadeEstoque);
        Assert.Equal(59.97m, result.Total);
        Assert.Equal(1, repo.Salvamentos);
    }

    [Fact]
    public async Task CheckoutVazioNaoSalva()
    {
        await Assert.ThrowsAsync<DomainException>(() => Service.FinalizarAsync(carrinho.Id, default));
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task FalhaDePersistenciaNaoRetornaSucessoNemTentaNovamente()
    {
        repo.FalhaAoSalvar = new ConflictException("Conflito simulado.");
        var error = await Assert.ThrowsAsync<ConflictException>(() => Service.AdicionarAsync(carrinho.Id, new(1, 1), default));
        Assert.Same(repo.FalhaAoSalvar, error);
        Assert.Equal(1, repo.Salvamentos);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1000001, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task PaginacaoInvalidaNaoConsultaRepositorio(int pagina, int tamanho)
    {
        await Assert.ThrowsAsync<DomainException>(() => Service.ListarAsync(pagina, tamanho, default));
        Assert.Equal(0, repo.Consultas);
    }

    [Fact]
    public async Task ListarEncaminhaPaginacaoEToken()
    {
        using var source = new CancellationTokenSource();
        var result = await Service.ListarAsync(1, 10, source.Token);
        Assert.Equal(carrinho.Id, Assert.Single(result).Id);
        Assert.Equal((1, 10), repo.Paginacao);
        Assert.Equal(source.Token, repo.UltimoToken);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task CancelamentoInterrompeAntesDeSalvar()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.AdicionarAsync(carrinho.Id, new(1, 1), source.Token));
        Assert.Empty(carrinho.Itens);
        Assert.Equal(0, repo.Salvamentos);
    }
}
