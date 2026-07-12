using Helios.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace Helios.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    b.UseVector();
                }));

        services.AddScoped<Helios.Application.Common.Interfaces.IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<Helios.Application.Common.Interfaces.ITextEmbeddingService, Helios.Infrastructure.Services.SemanticKernelEmbeddingService>();
        services.AddScoped<Helios.Application.Common.Interfaces.IProductService, Helios.Infrastructure.Services.ProductService>();

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration, params System.Reflection.Assembly[] consumerAssemblies)
    {
        services.AddMassTransit(x =>
        {
            if (consumerAssemblies != null && consumerAssemblies.Length > 0)
            {
                x.AddConsumers(consumerAssemblies);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                if (consumerAssemblies != null && consumerAssemblies.Length > 0)
                {
                    cfg.ReceiveEndpoint("ProductEmbeddingQueue", e =>
                    {
                        e.ConfigureConsumers(context);
                    });
                }
            });
        });

        return services;
    }
}
