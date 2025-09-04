using Clew.Application.Abstractions;
using Clew.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Clew.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<IContentSourceRouter, ContentSourceRouter>()
            .AddScoped<IProjectsService, ProjectsService>();
    }
}