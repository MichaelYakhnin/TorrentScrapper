using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using TorrentScrapper.Application.Abstractions;
using TorrentScrapper.Application.Contracts;

namespace TorrentScrapper.Parsing.Rutracker;

public class RutrackerParser : ITorrentParser
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RutrackerParser> _logger;
    private readonly RutrackerDetailClient _detailClient;
    private const int RateLimitDelayMs = 200; // 200ms delay between detail page requests

    public RutrackerParser(
        IHttpClientFactory httpClientFactory,
        ILogger<RutrackerParser> logger,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _detailClient = new RutrackerDetailClient(httpClientFactory, loggerFactory.CreateLogger<RutrackerDetailClient>());
    }

    public async Task<List<TorrentResultDto>> ParsePageAsync(int page, CancellationToken cancellationToken)
    {
        return await ParsePageAsync(null, page, cancellationToken);
    }

    public async Task<List<TorrentResultDto>> ParsePageAsync(string? categoryUrl, int page, CancellationToken cancellationToken)
    {
        var url = BuildListingUrl(categoryUrl, page);
        _logger.LogInformation("Fetching Rutracker category {Category} page {Page} from {Url}",
            categoryUrl ?? "all", page, url);

        var httpClient = _httpClientFactory.CreateClient("Rutracker");
        
        // Rutracker uses Windows-1251 encoding
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var html = Encoding.GetEncoding("windows-1251").GetString(bytes);

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html).Header("Content-Type", "text/html; charset=utf-8"), cancellationToken);

        var results = new List<TorrentResultDto>();

        // Rutracker listing page parsing
        var torrentLinks = document.QuerySelectorAll("a.torTopic");
        
        foreach (var link in torrentLinks)
        {
            var href = link.GetAttribute("href");
            if (string.IsNullOrEmpty(href))
                continue;

            var name = link.TextContent?.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            var torrentPageUrl = href.StartsWith("http")
                ? href
                : $"https://rutracker.org/forum/{href}";

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

        _logger.LogInformation("Parsed {Count} torrents from Rutracker category {Category} page {Page}",
            results.Count, categoryUrl ?? "all", page);
        return results;
    }

    private static string BuildListingUrl(string? categoryUrl, int page)
    {
        // If categoryUrl is provided, use it (e.g., "viewforum.php?f=7")
        if (!string.IsNullOrEmpty(categoryUrl))
        {
            var start = page == 1 ? 0 : (page - 1) * 50;
            return start == 0
                ? $"https://rutracker.org/forum/{categoryUrl}"
                : $"https://rutracker.org/forum/{categoryUrl}&start={start}";
        }

        // Default to category c=2 when no specific category
        if (page == 1)
            return "https://rutracker.org/forum/index.php?c=2";
        
        var defaultStart = (page - 1) * 50;
        return $"https://rutracker.org/forum/index.php?c=2&start={defaultStart}";
    }
}
