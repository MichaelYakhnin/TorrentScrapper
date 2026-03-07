using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using TorrentScrapper.Application.Abstractions;
using TorrentScrapper.Application.Contracts;

namespace TorrentScrapper.Parsing.Rutor;

public class RutorParser : ITorrentParser
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RutorParser> _logger;
    private readonly RutorDetailClient _detailClient;
    private const int RateLimitDelayMs = 200; // 200ms delay between detail page requests

    public RutorParser(
        IHttpClientFactory httpClientFactory,
        ILogger<RutorParser> logger,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _detailClient = new RutorDetailClient(httpClientFactory, loggerFactory.CreateLogger<RutorDetailClient>());
    }

    public async Task<List<TorrentResultDto>> ParsePageAsync(int page, CancellationToken cancellationToken)
    {
        return await ParsePageAsync(null, page, cancellationToken);
    }

    public async Task<List<TorrentResultDto>> ParsePageAsync(string? categoryUrl, int page, CancellationToken cancellationToken)
    {
        var url = BuildListingUrl(categoryUrl, page);
        _logger.LogInformation("Fetching Rutor category {Category} page {Page} from {Url}",
            categoryUrl ?? "all", page, url);

        var httpClient = _httpClientFactory.CreateClient("Rutor");
        var html = await httpClient.GetStringAsync(url, cancellationToken);

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html), cancellationToken);

        var results = new List<TorrentResultDto>();

        var torrentLinks = document.QuerySelectorAll("a[href*='/torrent/']");
        
        foreach (var link in torrentLinks)
        {
            var href = link.GetAttribute("href");
            if (string.IsNullOrEmpty(href) || !href.Contains("/torrent/"))
                continue;

            var name = link.TextContent?.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            var torrentPageUrl = href.StartsWith("http")
                ? href
                : $"https://rutor.info{href}";

            // Fetch detail page to extract image
            string? imageUrl = null;
            try
            {
                var detailHtml = await _detailClient.FetchDetailPageAsync(torrentPageUrl, cancellationToken);
                if (!string.IsNullOrEmpty(detailHtml))
                {
                    imageUrl = await _detailClient.ExtractImageUrlAsync(detailHtml, cancellationToken);
                }

                // Rate limiting: wait before next request
                await Task.Delay(RateLimitDelayMs, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch image for torrent: {Name}", name);
            }

            results.Add(new TorrentResultDto
            {
                Name = name,
                TorrentPageUrl = torrentPageUrl,
                ImageUrl = imageUrl
            });
        }

        _logger.LogInformation("Parsed {Count} torrents from Rutor category {Category} page {Page}",
            results.Count, categoryUrl ?? "all", page);
        return results;
    }

    private static string BuildListingUrl(string? categoryUrl, int page)
    {
        // Rutor uses /browse/{page}/{category}/{sort}/{quality} structure
        // page is 0-indexed: page 1 = 0, page 2 = 1, page 3 = 2, etc.
        // category: 1=foreign films, 4=foreign series, 5=russian films, 6=tv, 16=russian series
        // sort: 0=default, quality: 0=all
        
        var pageIndex = page - 1; // Convert to 0-indexed
        
        if (!string.IsNullOrEmpty(categoryUrl))
        {
            // categoryUrl contains the category ID (e.g., "1", "4", "5", "6", "16")
            return $"https://rutor.info/browse/{pageIndex}/{categoryUrl}/0/0";
        }

        // Default to category 0 (all) when no category specified
        return $"https://rutor.info/browse/{pageIndex}/0/0/0";
    }
}
