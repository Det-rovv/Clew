using System.Threading.RateLimiting;
using Clew.Application.Abstractions;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.ContentSources;
using Clew.Infrastructure.Services;
using Clew.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.RateLimiting;

namespace Clew.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var modrinthSection = configuration.GetSection(nameof(ModrinthSettings));
        var curseForgeSection = configuration.GetSection(nameof(CurseForgeSettings));
        var tasksConcurrencySettings = configuration.GetSection(nameof(ConcurrencySettings));
        var namingSettings = configuration.GetSection(nameof(ContentSourceNamingsSettings));
        
        serviceCollection.Configure<ModrinthSettings>(modrinthSection);
        serviceCollection.Configure<CurseForgeSettings>(curseForgeSection);
        serviceCollection.Configure<ConcurrencySettings>(tasksConcurrencySettings);
        serviceCollection.Configure<ContentSourceNamingsSettings>(namingSettings);
        
        var modrinthSettings = modrinthSection.Get<ModrinthSettings>();
        var curseForgeSettings = curseForgeSection.Get<CurseForgeSettings>();

        serviceCollection
            .AddHttpClient<IContentSource, ModrinthClient>(modrinthSettings.ApiName, client =>
            {
                client.BaseAddress = new Uri(modrinthSettings.BaseUrl);
            })
            .AddResilienceHandler("modrinth-pipeline", builder =>
            {
                var rateLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = modrinthSettings.MaxRequestsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 60,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10000
                });
                
                builder
                    .AddRateLimiter(new RateLimiterStrategyOptions
                    {
                        RateLimiter = _ => rateLimiter.AcquireAsync()
                    })
                    .AddTimeout(TimeSpan.FromSeconds(10))
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromSeconds(3),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true
                    });
            });

        serviceCollection
            .AddHttpClient<IContentSource, CurseForgeClient>(curseForgeSettings.ApiName, client =>
            {
                client.BaseAddress = new Uri(curseForgeSettings.BaseUrl);
                client.DefaultRequestHeaders.Add("x-api-key", curseForgeSettings.ApiKey);
            })
            .AddResilienceHandler("curseforge-pipeline", builder =>
            {
                builder
                    .AddTimeout(TimeSpan.FromSeconds(10))
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromSeconds(3),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true
                    });
            });
        
        serviceCollection.AddSingleton<IProjectListResolver, ParallelProjectListResolver>();
        serviceCollection.AddSingleton<IModrinthUrlFormatter, ModrinthUrlFormatter>();
        serviceCollection.AddSingleton<IContentSourceNamingsTranslator, ContentSourceNamingsTranslator>();
        serviceCollection.AddSingleton<IProjectVersionSelector, ProjectVersionSelector>();
        serviceCollection.AddSingleton<IThreadCountCalculator, ThreadCountCalculator>();
        
        return serviceCollection;
    }
}