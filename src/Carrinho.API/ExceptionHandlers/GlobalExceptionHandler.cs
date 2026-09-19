using Carrinho.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var isDomainError = exception is DomainException;
        if (!isDomainError) logger.LogError(exception, "Falha ao processar a requisição.");
        context.Response.StatusCode = isDomainError ? 422 : 500;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = isDomainError ? "Regra de negócio inválida" : "Erro interno",
            Detail = isDomainError ? exception.Message : "Não foi possível concluir a operação.",
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, options: (System.Text.Json.JsonSerializerOptions?)null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
