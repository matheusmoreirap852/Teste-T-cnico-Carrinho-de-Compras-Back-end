using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.UnitTests;

public sealed class CarrinhoTests
{
    [Fact]
    public void PrimeiraInclusaoUsaUmEProximasSomamQuantidade()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 10m, 20);
        c.Adicionar(p, 5);
        Assert.Equal(1, c.Itens.Single().Quantidade);
        c.Adicionar(p, 3);
        Assert.Equal(4, c.Itens.Single().Quantidade);
        Assert.Equal(40m, c.Total);
    }
    [Fact]
    public void QuantidadeSubstituiERecalculaCupom()
    {
        var c = new CarrinhoCompra();
        c.Adicionar(new Produto(1, "Produto", 19.99m, 20), 1);
        c.AplicarCupom(new Cupom(1, "10OFF", 10));
        c.AlterarQuantidade(1, 3);
        Assert.Equal(59.97m, c.Subtotal);
        Assert.Equal(6m, c.Desconto);
        Assert.Equal(53.97m, c.Total);
        c.AplicarCupom(new Cupom(2, "15OFF", 15));
        Assert.Equal(9m, c.Desconto);
        c.RemoverCupom();
        Assert.Equal(c.Subtotal, c.Total);
        c.RemoverItem(1);
        Assert.Equal(0, c.Total);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    public void RejeitaQuantidadeInvalidaOuSuperiorAoEstoque(int quantidade)
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 10, 3);
        Assert.Throws<DomainException>(() => c.Adicionar(p, quantidade));
        c.Adicionar(p, 1);
        Assert.Throws<DomainException>(() => c.AlterarQuantidade(1, quantidade));
    }
    [Fact]
    public void NaoPermiteSomaSuperiorAoEstoque()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 10, 2);
        c.Adicionar(p, 1);
        Assert.Throws<DomainException>(() => c.Adicionar(p, 2));
        Assert.Equal(1, c.Itens.Single().Quantidade);
    }
    [Fact]
    public void CheckoutBaixaEstoqueEBloqueiaTodasAlteracoes()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 10, 3);
        c.Adicionar(p, 1);
        c.Finalizar();
        Assert.Equal(2, p.QuantidadeEstoque);
        Assert.Throws<ConflictException>(() => c.Adicionar(p, 1));
        Assert.Throws<ConflictException>(() => c.AlterarQuantidade(1, 1));
        Assert.Throws<ConflictException>(() => c.RemoverItem(1));
        Assert.Throws<ConflictException>(() => c.AplicarCupom(new Cupom(1, "10OFF", 10)));
        Assert.Throws<ConflictException>(() => c.RemoverCupom());
        Assert.Throws<ConflictException>(() => c.Finalizar());
    }
    [Fact]
    public void CheckoutRevalidaEstoqueSemBaixaParcial()
    {
        var c = new CarrinhoCompra();
        var p1 = new Produto(1, "Um", 10, 3);
        var p2 = new Produto(2, "Dois", 10, 3);
        c.Adicionar(p1, 1);
        c.Adicionar(p2, 1);
        p2.Atualizar("Dois", 10, 0);
        Assert.Throws<DomainException>(() => c.Finalizar());
        Assert.Equal(3, p1.QuantidadeEstoque);
        Assert.Equal(StatusCarrinho.Aberto, c.Status);
    }
    [Fact]
    public void CheckoutVazioEItemInexistenteRetornamErro()
    {
        var c = new CarrinhoCompra();
        Assert.Throws<DomainException>(() => c.Finalizar());
        Assert.Throws<NotFoundException>(() => c.RemoverItem(123));
    }
    [Fact]
    public void AlteracaoDoCatalogoNaoMudaPrecoOuCupomJaAplicado()
    {
        var c = new CarrinhoCompra();
        var p = new Produto(1, "Produto", 10, 3);
        var cupom = new Cupom(1, "10OFF", 10);
        c.Adicionar(p, 1);
        c.AplicarCupom(cupom);
        p.Atualizar("Novo", 20, 3);
        cupom.Atualizar("OUTRO", 20);
        Assert.Equal(9m, c.Total);
        Assert.Equal("10OFF", c.CodigoCupom);
    }
}
