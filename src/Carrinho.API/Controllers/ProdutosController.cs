using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosController(ListarProdutos listarProdutos, ProdutosService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProdutoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponse>>> Listar(CancellationToken cancellationToken)
        => Ok(await listarProdutos.ExecutarAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Obter(int id, CancellationToken ct)
        => Ok(await service.ObterAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<ProdutoResponse>(201)]
    public async Task<ActionResult<ProdutoResponse>> Criar(CriarProdutoRequest request, CancellationToken ct)
    {
        var result = await service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Atualizar(int id, AtualizarProdutoRequest request, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
