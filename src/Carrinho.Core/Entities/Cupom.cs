using Carrinho.Core.Exceptions;

namespace Carrinho.Core.Entities;

public sealed class Cupom
{
    private Cupom() { }
    public Cupom(int id, string codigoCupom, decimal percentualDesconto)
    {
        if (id <= 0) throw new DomainException("O identificador deve ser positivo.");
        Id = id;
        Atualizar(codigoCupom, percentualDesconto);
    }
    public int Id { get; private set; }
    public string CodigoCupom { get; private set; } = "";
    public decimal PercentualDesconto { get; private set; }
    public void Atualizar(string codigoCupom, decimal percentualDesconto)
    {
        if (string.IsNullOrWhiteSpace(codigoCupom) || codigoCupom.Trim().Length > 50)
            throw new DomainException("Informe um código de cupom de até 50 caracteres.");
        if (percentualDesconto <= 0 || percentualDesconto > 100 || decimal.Round(percentualDesconto, 2) != percentualDesconto)
            throw new DomainException("O desconto deve estar entre 0,01 e 100, com até duas casas decimais.");
        CodigoCupom = codigoCupom.Trim().ToUpperInvariant();
        PercentualDesconto = percentualDesconto;
    }
}
