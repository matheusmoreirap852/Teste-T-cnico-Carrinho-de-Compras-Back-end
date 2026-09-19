using Carrinho.Core.Exceptions;

namespace Carrinho.Core.Entities;

public sealed class Produto
{
    private Produto() { }

    public Produto(int id, string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
        if (id <= 0) throw new DomainException("O identificador deve ser positivo.");
        Id = id;
        Atualizar(descricaoProduto, precoLiquido, quantidadeEstoque);
    }

    public void Atualizar(string descricaoProduto, decimal precoLiquido, int quantidadeEstoque)
    {
        if (string.IsNullOrWhiteSpace(descricaoProduto))
            throw new DomainException("A descrição do produto é obrigatória.");
        if (precoLiquido < 0) throw new DomainException("O preço não pode ser negativo.");
        if (precoLiquido > 9999999999999999.99m || decimal.Round(precoLiquido, 2) != precoLiquido)
            throw new DomainException("Informe um preço válido com até duas casas decimais.");
        if (quantidadeEstoque < 0) throw new DomainException("O estoque não pode ser negativo.");
        DescricaoProduto = descricaoProduto;
        PrecoLiquido = precoLiquido;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0 || quantidade > QuantidadeEstoque)
            throw new DomainException("Estoque insuficiente para finalizar o carrinho.");
        QuantidadeEstoque -= quantidade;
    }

    public int Id { get; private set; }
    public string DescricaoProduto { get; private set; } = string.Empty;
    public decimal PrecoLiquido { get; private set; }
    public int QuantidadeEstoque { get; private set; }
}
