using Carrinho.Application.DTOs;
using Carrinho.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.Controllers;

[ApiController]
[Route("api/cupons")]
public sealed class CuponsController(CuponsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CupomResponse>>> Listar(CancellationToken ct)
        => Ok(await service.ListarAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CupomResponse>> Obter(int id, CancellationToken ct)
        => Ok(await service.ObterAsync(id, ct));
    [HttpPost]
    [ProducesResponseType<CupomResponse>(201)]
    public async Task<ActionResult<CupomResponse>> Criar(CriarCupomRequest request, CancellationToken ct)
    {
        var result = await service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CupomResponse>> Atualizar(int id, AtualizarCupomRequest request, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, request, ct));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
