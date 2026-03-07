using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TorrentScrapper.Infrastructure.Http;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddScraperHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userAgent = configuration["Scraping:UserAgent"] 
            ?? "TorrentMetadataBrowser/1.0";
        var timeoutSeconds = configuration.GetValue<int>("Scraping:RequestTimeout", 30);

        // Named HTTP client for Rutor
        services.AddHttpClient("Rutor", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        // Named HTTP client for Rutracker
        services.AddHttpClient("Rutracker", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        return services;
    }
}
