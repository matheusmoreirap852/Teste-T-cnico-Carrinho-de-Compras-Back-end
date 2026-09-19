using System.ComponentModel.DataAnnotations;

namespace Carrinho.Application.DTOs;

public sealed record CriarProdutoRequest([Range(1, int.MaxValue)] int Id, [Required] string DescricaoProduto, decimal PrecoLiquido, int QuantidadeEstoque);
public sealed record AtualizarProdutoRequest([Required] string DescricaoProduto, decimal PrecoLiquido, int QuantidadeEstoque);
public sealed record CriarCupomRequest([Range(1, int.MaxValue)] int Id, [Required] string CodigoCupom, decimal PercentualDesconto);
public sealed record AtualizarCupomRequest([Required] string CodigoCupom, decimal PercentualDesconto);
public sealed record AdicionarItemRequest([Range(1, int.MaxValue)] int ProdutoId, [Range(1, int.MaxValue)] int Quantidade);
public sealed record QuantidadeRequest([Range(1, int.MaxValue)] int Quantidade);
public sealed record AplicarCupomRequest([Required] string CodigoCupom);
