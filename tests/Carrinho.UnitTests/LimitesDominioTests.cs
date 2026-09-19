using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.UnitTests;

public sealed class LimitesDominioTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IdInvalidoERejeitado(int id)
    {
        Assert.Throws<DomainException>(() => new Produto(id, "Produto", 10, 1));
        Assert.Throws<DomainException>(() => new Cupom(id, "10OFF", 10));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DescricaoECodigoObrigatorios(string? valor)
    {
        Assert.Throws<DomainException>(() => new Produto(1, valor!, 10, 1));
        Assert.Throws<DomainException>(() => new Cupom(1, valor!, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(10.001)]
    public void PercentualInvalidoNaoAlteraCupom(decimal percentual)
    {
        var cupom = new Cupom(1, "10OFF", 10);
        Assert.Throws<DomainException>(() => cupom.Atualizar("NOVO", percentual));
        Assert.Equal("10OFF", cupom.CodigoCupom);
        Assert.Equal(10, cupom.PercentualDesconto);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    public void PercentualNosLimitesEValido(decimal percentual)
        => Assert.Equal(percentual, new Cupom(1, "PROMO", percentual).PercentualDesconto);

    [Fact]
    public void CodigoAceitaCinquentaCaracteresMasNaoCinquentaEUm()
    {
        Assert.Equal(50, new Cupom(1, new string('A', 50), 10).CodigoCupom.Length);
        Assert.Throws<DomainException>(() => new Cupom(1, new string('A', 51), 10));
    }

    [Fact]
    public void PrecoRespeitaPrecisaoELimiteDoBanco()
    {
        Assert.Throws<DomainException>(() => new Produto(1, "Produto", 0.001m, 1));
        Assert.Throws<DomainException>(() => new Produto(1, "Produto", 10000000000000000m, 1));
        Assert.Equal(9999999999999999.99m, new Produto(1, "Produto", 9999999999999999.99m, 1).PrecoLiquido);
        Assert.Equal(0m, new Produto(1, "Grátis", 0m, 1).PrecoLiquido);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void BaixaInvalidaPreservaEstoque(int quantidade)
    {
        var produto = new Produto(1, "Produto", 10, 5);
        Assert.Throws<DomainException>(() => produto.BaixarEstoque(quantidade));
        Assert.Equal(5, produto.QuantidadeEstoque);
    }

    [Fact]
    public void UltimaUnidadePodeSerComprada()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Último", 10, 1);
        c.Adicionar(p, 1);
        c.Finalizar();
        Assert.Equal(0, p.QuantidadeEstoque);
        Assert.Equal(10, c.Total);
    }

    [Fact]
    public void SemEstoqueNaoAdiciona()
    {
        var c = new CarrinhoCompra();
        Assert.Throws<DomainException>(() => c.Adicionar(new Produto(1, "Produto", 10, 0), 1));
        Assert.Empty(c.Itens);
    }

    [Fact]
    public void SomaDeQuantidadeNaoEstouraInteiro()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 1, int.MaxValue);
        c.Adicionar(p, 1);
        c.AlterarQuantidade(1, int.MaxValue);
        Assert.Throws<DomainException>(() => c.Adicionar(p, 1));
        Assert.Equal(int.MaxValue, c.Itens.Single().Quantidade);
    }

    [Theory]
    [InlineData(0.05, 10, 0.01, 0.04)]
    [InlineData(0.03, 15, 0.00, 0.03)]
    [InlineData(19.99, 15, 3.00, 16.99)]
    [InlineData(19.99, 100, 19.99, 0.00)]
    public void ArredondamentoEDescontoIntegral(decimal preco, decimal percentual, decimal desconto, decimal total)
    {
        var c = new CarrinhoCompra();
        c.Adicionar(new Produto(1, "Produto", preco, 1), 1);
        c.AplicarCupom(new Cupom(1, "PROMO", percentual));
        Assert.Equal(desconto, c.Desconto);
        Assert.Equal(total, c.Total);
    }

    [Fact]
    public void CupomEmCarrinhoVazioMantemTotaisZerados()
    {
        var c = new CarrinhoCompra();
        c.AplicarCupom(new Cupom(1, "10OFF", 10));
        Assert.Equal(0, c.Subtotal);
        Assert.Equal(0, c.Desconto);
        Assert.Equal(0, c.Total);
        c.RemoverCupom();
        c.RemoverCupom();
        Assert.Null(c.CupomId);
    }

    [Fact]
    public void MultiplosProdutosRecalculamAposRemocaoECheckout()
    {
        var c = new CarrinhoCompra();
        var p1 = new Produto(1, "Café", 12.50m, 10);
        var p2 = new Produto(2, "Leite", 5.25m, 10);
        c.Adicionar(p1, 1);
        c.Adicionar(p2, 1);
        c.AlterarQuantidade(1, 2);
        c.AlterarQuantidade(2, 3);
        c.AplicarCupom(new Cupom(1, "10OFF", 10));
        Assert.Equal(40.75m, c.Subtotal);
        Assert.Equal(4.08m, c.Desconto);
        Assert.Equal(36.67m, c.Total);
        c.RemoverItem(2);
        Assert.Equal(22.50m, c.Total);
        c.Finalizar();
        Assert.Equal(8, p1.QuantidadeEstoque);
        Assert.Equal(10, p2.QuantidadeEstoque);
    }
}
