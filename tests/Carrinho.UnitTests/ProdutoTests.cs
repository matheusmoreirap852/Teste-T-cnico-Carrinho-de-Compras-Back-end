using Carrinho.Core.Entities;
using Carrinho.Core.Exceptions;

namespace Carrinho.UnitTests;

public sealed class ProdutoTests
{
    [Theory]
    [InlineData(-0.01, 1)]
    [InlineData(10, -1)]
    public void NaoPermitePrecoOuEstoqueNegativo(decimal preco, int estoque)
        => Assert.Throws<DomainException>(() => new Produto(1, "Produto", preco, estoque));

    [Fact]
    public void PermiteProdutoSemEstoque()
    {
        var produto = new Produto(1, "Produto", 19.99m, 0);
        Assert.Equal(0, produto.QuantidadeEstoque);
        Assert.Equal(19.99m, produto.PrecoLiquido);
    }
}
