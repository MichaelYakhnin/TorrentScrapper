using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TorrentScrapper.Application.Abstractions;
using TorrentScrapper.Application.Contracts;

namespace TorrentScrapper.Application.Services;

public interface ITorrentService
{
    Task<PagedTorrentResponseDto> GetTorrentsAsync(
        TorrentSite site,
        int page,
        CancellationToken cancellationToken);
    
    Task<PagedTorrentResponseDto> GetTorrentsAsync(
        TorrentSite site,
        string? categoryUrl,
        int page,
        CancellationToken cancellationToken);
}

public class TorrentService : ITorrentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TorrentService> _logger;
    private readonly int _cacheExpirationMinutes;

    public TorrentService(
        IServiceProvider serviceProvider,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<TorrentService> logger)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _logger = logger;
        _cacheExpirationMinutes = configuration.GetValue<int>("Scraping:CacheExpirationMinutes", 5);
    }

    public async Task<PagedTorrentResponseDto> GetTorrentsAsync(
        TorrentSite site,
        int page,
        CancellationToken cancellationToken)
    {
        return await GetTorrentsAsync(site, null, page, cancellationToken);
    }

    public async Task<PagedTorrentResponseDto> GetTorrentsAsync(
        TorrentSite site,
        string? categoryUrl,
        int page,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page number must be >= 1", nameof(page));
        }

        var cacheKey = string.IsNullOrEmpty(categoryUrl)
            ? $"{site}_{page}"
            : $"{site}_{categoryUrl}_{page}";

        // Try to get from cache first
        if (_cache.TryGetValue<PagedTorrentResponseDto>(cacheKey, out var cachedResult)
            && cachedResult != null)
        {
            _logger.LogInformation("Returning cached results for {Site} category {Category} page {Page}",
                site, categoryUrl ?? "all", page);
            return cachedResult;
        }

        // Get the appropriate parser
        var parser = _serviceProvider.GetKeyedService<ITorrentParser>(site.ToString());
        if (parser == null)
        {
            throw new InvalidOperationException($"No parser registered for site: {site}");
        }

        _logger.LogInformation("Fetching torrents from {Site} category {Category} page {Page}",
            site, categoryUrl ?? "all", page);
        var results = await parser.ParsePageAsync(categoryUrl, page, cancellationToken);

        var response = new PagedTorrentResponseDto
        {
            Site = site.ToString().ToLowerInvariant(),
            Page = page,
            Results = results
        };

        // Cache the results
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheExpirationMinutes));
        
        _cache.Set(cacheKey, response, cacheOptions);

        return response;
    }
}
