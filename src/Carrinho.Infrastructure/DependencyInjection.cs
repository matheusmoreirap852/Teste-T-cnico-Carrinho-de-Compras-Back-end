using Carrinho.Application.Interfaces;
using Carrinho.Infrastructure.Persistence;
using Carrinho.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Carrinho.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CarrinhoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        return services;
    }
}
