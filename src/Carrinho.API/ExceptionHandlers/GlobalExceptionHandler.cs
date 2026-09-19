using Carrinho.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Carrinho.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var isDomainError = exception is DomainException or NotFoundException or ConflictException;
        if (!isDomainError) logger.LogError(exception, "Falha ao processar a requisição.");
        context.Response.StatusCode = exception switch
        {
            NotFoundException => 404,
            ConflictException => 409,
            DomainException => 422,
            _ => 500
        };
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = exception switch
            {
                NotFoundException => "Recurso não encontrado",
                ConflictException => "Conflito de operação",
                DomainException => "Regra de negócio inválida",
                _ => "Erro interno"
            },
            Detail = isDomainError ? exception.Message : "Não foi possível concluir a operação.",
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, options: (System.Text.Json.JsonSerializerOptions?)null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
