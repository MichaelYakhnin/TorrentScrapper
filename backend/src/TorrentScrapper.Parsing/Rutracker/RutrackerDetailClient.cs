using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace TorrentScrapper.Parsing.Rutracker;

public class RutrackerDetailClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RutrackerDetailClient> _logger;

    public RutrackerDetailClient(
        IHttpClientFactory httpClientFactory,
        ILogger<RutrackerDetailClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> FetchDetailPageAsync(string detailUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching Rutracker detail page: {Url}", detailUrl);
            
            var httpClient = _httpClientFactory.CreateClient("Rutracker");
            
            // Rutracker uses Windows-1251 encoding
            var response = await httpClient.GetAsync(detailUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var html = Encoding.GetEncoding("windows-1251").GetString(bytes);
            
            return html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Rutracker detail page: {Url}", detailUrl);
            return null;
        }
    }

    public async Task<string?> ExtractImageUrlAsync(string html, CancellationToken cancellationToken)
    {
        try
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(html), cancellationToken);

            // Try to find a poster image in the detail page
            // Rutracker typically shows images in the post body
            var imageSelectors = new[]
            {
                ".post-body img[src]",        // Images in post body
                "var.postImg img[src]",       // Poster images
                ".post-content img[src]",     // Images in content
                "img[src*='postimg']",        // Common image hosts
                "img[src*='imgur']"
            };

            foreach (var selector in imageSelectors)
            {
                var image = document.QuerySelector(selector);
                if (image != null)
                {
                    var src = image.GetAttribute("src");
                    if (!string.IsNullOrEmpty(src) && IsValidImageUrl(src))
                    {
                        _logger.LogDebug("Found Rutracker image: {ImageUrl}", src);
                        return MakeAbsoluteUrl(src);
                    }
                }
            }

            _logger.LogDebug("No suitable image found in Rutracker detail page");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract image from Rutracker detail page");
            return null;
        }
    }

    private static bool IsValidImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        var lowerUrl = url.ToLowerInvariant();
        return lowerUrl.EndsWith(".jpg") || 
               lowerUrl.EndsWith(".jpeg") || 
               lowerUrl.EndsWith(".png") || 
               lowerUrl.EndsWith(".gif") ||
               lowerUrl.Contains(".jpg?") ||
               lowerUrl.Contains(".jpeg?") ||
               lowerUrl.Contains(".png?");
    }

    private static string MakeAbsoluteUrl(string url)
    {
        if (url.StartsWith("http://") || url.StartsWith("https://"))
            return url;
        
        if (url.StartsWith("//"))
            return "https:" + url;
        
        if (url.StartsWith("/"))
            return "https://rutracker.org" + url;
        
        return url;
    }
}
