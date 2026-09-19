namespace Carrinho.Core.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
