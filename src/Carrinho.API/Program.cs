using Carrinho.API.ExceptionHandlers;
using Carrinho.Application.UseCases;
using Carrinho.Infrastructure;
using Carrinho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<ListarProdutos>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Carrinho")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:Carrinho via user-secrets ou variável de ambiente."));

var app = builder.Build();
if (args.Contains("--migrate"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<CarrinhoDbContext>().Database.MigrateAsync();
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("../openapi/v1.json", "Carrinho de Compras API v1");
        options.DocumentTitle = "Carrinho de Compras — Swagger";
    });
}
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", async (CarrinhoDbContext db, CancellationToken ct) =>
{
    try
    {
        await db.Produtos.AsNoTracking().OrderBy(p => p.Id).Take(1).ToListAsync(ct);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception) when (!ct.IsCancellationRequested)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapControllers();
app.Run();
