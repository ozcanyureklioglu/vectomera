using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Helios.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation: tüm validator'ları otomatik tarar ve kaydeder
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
