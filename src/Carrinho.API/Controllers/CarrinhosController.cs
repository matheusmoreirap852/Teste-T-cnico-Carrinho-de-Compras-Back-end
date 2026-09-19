using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.Controllers;

[ApiController]
[Route("api/carrinhos")]
public sealed class CarrinhosController(CarrinhosService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CarrinhoResponse>>> Listar(CancellationToken ct, int pagina = 1, int tamanho = 20)
        => Ok(await service.ListarAsync(pagina, tamanho, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CarrinhoResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await service.ObterAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<CarrinhoResponse>(201)]
    public async Task<ActionResult<CarrinhoResponse>> Criar(CancellationToken ct)
    {
        var result = await service.CriarAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
    [HttpPost("{id:guid}/itens")]
    public async Task<ActionResult<CarrinhoResponse>> Adicionar(Guid id, AdicionarItemRequest request, CancellationToken ct)
        => Ok(await service.AdicionarAsync(id, request, ct));
    [HttpPut("{id:guid}/itens/{produtoId:int}")]
    public async Task<ActionResult<CarrinhoResponse>> Quantidade(Guid id, int produtoId, QuantidadeRequest request, CancellationToken ct)
        => Ok(await service.QuantidadeAsync(id, produtoId, request.Quantidade, ct));
    [HttpDelete("{id:guid}/itens/{produtoId:int}")]
    public async Task<ActionResult<CarrinhoResponse>> RemoverItem(Guid id, int produtoId, CancellationToken ct)
        => Ok(await service.RemoverItemAsync(id, produtoId, ct));
    [HttpPut("{id:guid}/cupom")]
    public async Task<ActionResult<CarrinhoResponse>> AplicarCupom(Guid id, AplicarCupomRequest request, CancellationToken ct)
        => Ok(await service.AplicarCupomAsync(id, request.CodigoCupom, ct));
    [HttpDelete("{id:guid}/cupom")]
    public async Task<ActionResult<CarrinhoResponse>> RemoverCupom(Guid id, CancellationToken ct)
        => Ok(await service.RemoverCupomAsync(id, ct));
    [HttpPost("{id:guid}/checkout")]
    public async Task<ActionResult<CarrinhoResponse>> Finalizar(Guid id, CancellationToken ct)
        => Ok(await service.FinalizarAsync(id, ct));
}
