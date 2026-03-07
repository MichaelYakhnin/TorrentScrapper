using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace TorrentScrapper.Parsing.Rutor;

public class RutorDetailClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RutorDetailClient> _logger;

    public RutorDetailClient(
        IHttpClientFactory httpClientFactory,
        ILogger<RutorDetailClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> FetchDetailPageAsync(string detailUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching Rutor detail page: {Url}", detailUrl);
            
            var httpClient = _httpClientFactory.CreateClient("Rutor");
            var html = await httpClient.GetStringAsync(detailUrl, cancellationToken);
            
            return html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Rutor detail page: {Url}", detailUrl);
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
            // Look for images in common locations
            var imageSelectors = new[]
            {
                "img[src*='postimg']",        // Common image hosts
                "img[src*='imgur']",
                "img[src*='imageshack']",
                "td img[src]",                // Images in table cells
                "#details img[src]"           // Images in details section
            };

            foreach (var selector in imageSelectors)
            {
                var image = document.QuerySelector(selector);
                if (image != null)
                {
                    var src = image.GetAttribute("src");
                    if (!string.IsNullOrEmpty(src) && IsValidImageUrl(src))
                    {
                        _logger.LogDebug("Found Rutor image: {ImageUrl}", src);
                        return MakeAbsoluteUrl(src);
                    }
                }
            }

            _logger.LogDebug("No suitable image found in Rutor detail page");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract image from Rutor detail page");
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
            return "https://rutor.info" + url;
        
        return url;
    }
}
