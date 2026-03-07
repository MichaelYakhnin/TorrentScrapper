using System.Text;
using System.Linq;
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

            // 1) Prefer <var class="postImg" title="..."> which holds external image URLs on some posts
            var varPostImg = document.QuerySelector("var.postImg") as IElement;
            if (varPostImg != null)
            {
                var title = varPostImg.GetAttribute("title");
                if (!string.IsNullOrEmpty(title) && IsValidImageUrl(title))
                {
                    _logger.LogDebug("Found Rutracker image in <var.postImg> title: {ImageUrl}", title);
                    return MakeAbsoluteUrl(title);
                }
            }

            // 2) Direct <img> elements with 'postImg' related classes
            var imgPost = document.QuerySelector("img.postImg, img.postImgAligned, img.postImgAlignedLeft, img.postImgAlignedRight, img.postImgInline") as IElement;
            if (imgPost != null)
            {
                var src = imgPost.GetAttribute("src") ?? imgPost.GetAttribute("data-src") ?? imgPost.GetAttribute("data-original");
                if (!string.IsNullOrEmpty(src) && IsValidImageUrl(src))
                {
                    _logger.LogDebug("Found Rutracker image in img.postImg: {ImageUrl}", src);
                    return MakeAbsoluteUrl(src);
                }
            }

            // Broad set of selectors observed on rutracker detail pages
            var imageSelectors = new[]
            {
                "div.postbody img[src]",
                "div.post-body img[src]",
                "td.postbody img[src]",
                "div.postMsg img[src]",
                ".post-content img[src]",
                ".post-body img[src]",
                ".postImg img[src]",
                "img[src*='postimg']",
                "img[src*='imgur']",
                "img[data-src]",
                "img[data-original]",
                "img[src]"
            };

            foreach (var selector in imageSelectors)
            {
                var image = document.QuerySelector(selector) as IElement;
                if (image == null)
                    continue;

                // Try common attributes that may contain the actual image URL
                var src = image.GetAttribute("src")
                          ?? image.GetAttribute("data-src")
                          ?? image.GetAttribute("data-original")
                          ?? image.GetAttribute("data-lazy")
                          ?? image.GetAttribute("data-actualsrc")
                          ?? image.GetAttribute("srcset")
                          ?? image.GetAttribute("data-srcset");

                if (string.IsNullOrEmpty(src))
                    continue;

                // If srcset provided, take the first URL
                if (src.Contains(',') || src.Contains(' '))
                {
                    // srcset can be like "url1 1x, url2 2x" or multiple comma-separated
                    var parts = src.Split(',').Select(p => p.Trim()).ToArray();
                    if (parts.Length > 0)
                    {
                        // each part may contain a URL and descriptor ("url 1x"); take the URL portion
                        var first = parts[0].Split(' ')[0].Trim();
                        if (!string.IsNullOrEmpty(first))
                            src = first;
                    }
                }

                if (IsValidImageUrl(src))
                {
                    _logger.LogDebug("Found Rutracker image: {ImageUrl}", src);
                    return MakeAbsoluteUrl(src);
                }
            }

            // Fallback: anchors linking directly to images
            var anchor = document.QuerySelector("a[href*='.jpg'], a[href*='.jpeg'], a[href*='.png'], a[href*='.gif']") as IElement;
            if (anchor != null)
            {
                var href = anchor.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && IsValidImageUrl(href))
                {
                    _logger.LogDebug("Found image via anchor: {ImageUrl}", href);
                    return MakeAbsoluteUrl(href);
                }
            }

            // Fallback: elements with inline background-image style
            var styled = document.QuerySelector("[style*='background-image']") as IElement;
            if (styled != null)
            {
                var style = styled.GetAttribute("style");
                if (!string.IsNullOrEmpty(style))
                {
                    var start = style.IndexOf("url(", StringComparison.OrdinalIgnoreCase);
                    if (start >= 0)
                    {
                        start += 4; // move past "url("
                        var end = style.IndexOf(')', start);
                        if (end > start)
                        {
                            var url = style[start..end].Trim(' ', '\'', '"');
                            if (IsValidImageUrl(url))
                            {
                                _logger.LogDebug("Found image via background-image style: {ImageUrl}", url);
                                return MakeAbsoluteUrl(url);
                            }
                        }
                    }
                }
            }

            // As a last resort, log a short snippet of the page for investigation
            try
            {
                var snippet = html.Length > 1000 ? html.Substring(0, 1000) : html;
                _logger.LogDebug("Rutracker detail page snippet for inspection: {Snippet}", snippet);
            }
            catch { }

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

        // Accept absolute URLs and protocol-relative URLs
        if (lowerUrl.StartsWith("http://") || lowerUrl.StartsWith("https://") || lowerUrl.StartsWith("//"))
            return true;

        // Accept root-relative paths
        if (lowerUrl.StartsWith("/"))
            return true;

        // Accept common image extensions
        if (lowerUrl.EndsWith(".jpg") || lowerUrl.EndsWith(".jpeg") || lowerUrl.EndsWith(".png") || lowerUrl.EndsWith(".gif")
            || lowerUrl.Contains(".jpg?") || lowerUrl.Contains(".jpeg?") || lowerUrl.Contains(".png?"))
            return true;

        // Accept known image hosts or identifiers even if extension missing
        if (lowerUrl.Contains("postimg") || lowerUrl.Contains("imgur") || lowerUrl.Contains("images") || lowerUrl.Contains("uploads") || lowerUrl.Contains("cdn"))
            return true;

        return false;
    }

    private static string MakeAbsoluteUrl(string url)
    {
        if (url.StartsWith("http://") || url.StartsWith("https://"))
            return url;
        
        if (url.StartsWith("//"))
            return "https:" + url;
        
        if (url.StartsWith("/"))
            return "https://rutracker.org" + url;

        // Some detail pages use relative paths without a leading slash; make them relative to the forum base
        return "https://rutracker.org/forum/" + url;
    }
}
