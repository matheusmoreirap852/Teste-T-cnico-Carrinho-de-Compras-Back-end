using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;
using Carrinho.UnitTests.Support;

namespace Carrinho.UnitTests;

public sealed class CatalogoServiceTests
{
    [Fact]
    public async Task CriarProdutoEncaminhaDadosESalvaUmaVez()
    {
        var repo = new LojaRepositoryFake();
        using var source = new CancellationTokenSource();
        var result = await new ProdutosService(repo).CriarAsync(new(42, "Café", 25.50m, 12), source.Token);
        Assert.Equal(new ProdutoResponse(42, "Café", 25.50m, 12), result);
        var entity = Assert.IsType<Produto>(Assert.Single(repo.Adicionados));
        Assert.Equal(result.PrecoLiquido, entity.PrecoLiquido);
        Assert.Equal(1, repo.Salvamentos);
        Assert.Equal(source.Token, repo.UltimoToken);
    }

    [Fact]
    public async Task ProdutoDuplicadoNaoAdicionaNemSalva()
    {
        var repo = new LojaRepositoryFake();
        repo.Produtos[1] = new(1, "Original", 10, 5);
        await Assert.ThrowsAsync<ConflictException>(() => new ProdutosService(repo).CriarAsync(new(1, "Outro", 20, 6), default));
        Assert.Empty(repo.Adicionados);
        Assert.Equal(0, repo.Salvamentos);
        Assert.Equal("Original", repo.Produtos[1].DescricaoProduto);
    }

    [Theory]
    [InlineData("obter")]
    [InlineData("atualizar")]
    [InlineData("remover")]
    public async Task ProdutoInexistenteRetornaNotFound(string operation)
    {
        var repo = new LojaRepositoryFake();
        var service = new ProdutosService(repo);
        await Assert.ThrowsAsync<NotFoundException>(() => operation switch
        {
            "obter" => service.ObterAsync(1, default),
            "atualizar" => service.AtualizarAsync(1, new("Produto", 10, 1), default),
            _ => service.RemoverAsync(1, default)
        });
        Assert.Empty(repo.Removidos);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task AtualizarProdutoInvalidoPreservaDadosENaoSalva()
    {
        var repo = new LojaRepositoryFake();
        repo.Produtos[1] = new(1, "Original", 10, 5);
        await Assert.ThrowsAsync<DomainException>(() => new ProdutosService(repo).AtualizarAsync(1, new("Novo", 20, -1), default));
        Assert.Equal("Original", repo.Produtos[1].DescricaoProduto);
        Assert.Equal(10m, repo.Produtos[1].PrecoLiquido);
        Assert.Equal(5, repo.Produtos[1].QuantidadeEstoque);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task ConsultarAtualizarERemoverProduto()
    {
        var repo = new LojaRepositoryFake();
        repo.Produtos[1] = new(1, "Original", 10, 5);
        var service = new ProdutosService(repo);
        Assert.Equal("Original", (await service.ObterAsync(1, default)).DescricaoProduto);
        var result = await service.AtualizarAsync(1, new("Novo", 30, 9), default);
        Assert.Equal(new ProdutoResponse(1, "Novo", 30, 9), result);
        await service.RemoverAsync(1, default);
        Assert.Same(repo.Produtos[1], Assert.Single(repo.Removidos));
        Assert.Equal(2, repo.Salvamentos);
    }

    [Fact]
    public async Task ListarProdutosVazioEComEstoqueZero()
    {
        var repo = new LojaRepositoryFake();
        var service = new ListarProdutos(repo);
        Assert.Empty(await service.ExecutarAsync(default));
        repo.Produtos[1] = new(1, "Sem estoque", 12.34m, 0);
        repo.Produtos[2] = new(2, "Disponível", 50m, 4);
        var result = await service.ExecutarAsync(default);
        Assert.Collection(result,
            p => Assert.Equal(new ProdutoResponse(1, "Sem estoque", 12.34m, 0), p),
            p => Assert.Equal(new ProdutoResponse(2, "Disponível", 50m, 4), p));
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task CupomDuplicadoNaoAdicionaNemSalva()
    {
        var repo = new LojaRepositoryFake();
        repo.Cupons[1] = new(1, "10OFF", 10);
        await Assert.ThrowsAsync<ConflictException>(() => new CuponsService(repo).CriarAsync(new(1, "15OFF", 15), default));
        Assert.Empty(repo.Adicionados);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Theory]
    [InlineData("obter")]
    [InlineData("atualizar")]
    [InlineData("remover")]
    public async Task CupomInexistenteRetornaNotFound(string operation)
    {
        var repo = new LojaRepositoryFake();
        var service = new CuponsService(repo);
        await Assert.ThrowsAsync<NotFoundException>(() => operation switch
        {
            "obter" => service.ObterAsync(1, default),
            "atualizar" => service.AtualizarAsync(1, new("10OFF", 10), default),
            _ => service.RemoverAsync(1, default)
        });
        Assert.Empty(repo.Removidos);
        Assert.Equal(0, repo.Salvamentos);
    }

    [Fact]
    public async Task CriarCupomNormalizaCodigoEPersiste()
    {
        var repo = new LojaRepositoryFake();
        var result = await new CuponsService(repo).CriarAsync(new(1, "  promo  ", 12.50m), default);
        Assert.Equal(new CupomResponse(1, "PROMO", 12.50m), result);
        Assert.Equal("PROMO", Assert.IsType<Cupom>(Assert.Single(repo.Adicionados)).CodigoCupom);
        Assert.Equal(1, repo.Salvamentos);
    }

    [Fact]
    public async Task ListarConsultarAtualizarERemoverCupom()
    {
        var repo = new LojaRepositoryFake();
        var service = new CuponsService(repo);
        Assert.Empty(await service.ListarAsync(default));
        repo.Cupons[1] = new(1, "10OFF", 10);
        Assert.Equal("10OFF", Assert.Single(await service.ListarAsync(default)).CodigoCupom);
        Assert.Equal(10m, (await service.ObterAsync(1, default)).PercentualDesconto);
        var result = await service.AtualizarAsync(1, new(" desconto ", 15), default);
        Assert.Equal(new CupomResponse(1, "DESCONTO", 15), result);
        await service.RemoverAsync(1, default);
        Assert.Same(repo.Cupons[1], Assert.Single(repo.Removidos));
        Assert.Equal(2, repo.Salvamentos);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConflitoDeVinculoNaExclusaoEPropagado(bool produto)
    {
        var repo = new LojaRepositoryFake { FalhaAoSalvar = new ConflictException("Registro vinculado.") };
        repo.Produtos[1] = new(1, "Produto", 10, 1);
        repo.Cupons[1] = new(1, "10OFF", 10);
        var error = await Assert.ThrowsAsync<ConflictException>(() => produto
            ? new ProdutosService(repo).RemoverAsync(1, default)
            : new CuponsService(repo).RemoverAsync(1, default));
        Assert.Same(repo.FalhaAoSalvar, error);
        Assert.Equal(1, repo.Salvamentos);
    }
}
