namespace Carrinho.Core.Entities;

public sealed class ItemCarrinho
{
    private ItemCarrinho() { }
    internal ItemCarrinho(Guid carrinhoId, Produto produto)
    {
        CarrinhoId = carrinhoId;
        ProdutoId = produto.Id;
        Produto = produto;
        PrecoLiquido = produto.PrecoLiquido;
        DescricaoProduto = produto.DescricaoProduto;
        Quantidade = 1;
    }
    public Guid CarrinhoId { get; private set; }
    public int ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public string DescricaoProduto { get; private set; } = "";
    public int Quantidade { get; private set; }
    public decimal PrecoLiquido { get; private set; }
    public decimal PrecoItem => PrecoLiquido * Quantidade;
    internal void DefinirQuantidade(int quantidade) => Quantidade = quantidade;
}
