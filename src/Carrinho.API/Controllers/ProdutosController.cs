using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosController(ListarProdutos listarProdutos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProdutoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponse>>> Listar(CancellationToken cancellationToken)
        => Ok(await listarProdutos.ExecutarAsync(cancellationToken));
}
