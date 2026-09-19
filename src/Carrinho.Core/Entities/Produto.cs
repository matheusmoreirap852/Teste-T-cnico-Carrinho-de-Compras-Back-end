using Carrinho.Core.Exceptions;

namespace Carrinho.Core.Entities;

public sealed class Produto
{
    private Produto() { }

    public Produto(int id, string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
        if (id <= 0) throw new DomainException("O identificador deve ser positivo.");
        if (string.IsNullOrWhiteSpace(descricaoProduto))
            throw new DomainException("A descrição do produto é obrigatória.");
        if (precoLiquido < 0) throw new DomainException("O preço não pode ser negativo.");
        if (quantidadeEstoque < 0) throw new DomainException("O estoque não pode ser negativo.");
        Id = id;
        DescricaoProduto = descricaoProduto;
        PrecoLiquido = precoLiquido;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public int Id { get; private set; }
    public string DescricaoProduto { get; private set; } = string.Empty;
    public decimal PrecoLiquido { get; private set; }
    public int QuantidadeEstoque { get; private set; }
}
