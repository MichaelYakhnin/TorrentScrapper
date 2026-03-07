using Microsoft.AspNetCore.Mvc;
using TorrentScrapper.Application.Contracts;
using TorrentScrapper.Application.Services;

namespace TorrentScrapper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TorrentsController : ControllerBase
{
    private readonly ITorrentService _torrentService;
    private readonly ILogger<TorrentsController> _logger;

    public TorrentsController(
        ITorrentService torrentService,
        ILogger<TorrentsController> logger)
    {
        _torrentService = torrentService;
        _logger = logger;
    }

    [HttpGet("categories")]
    public ActionResult<CategoriesResponseDto> GetCategories([FromQuery] string site)
    {
        // Validate site parameter
        if (!Enum.TryParse<TorrentSite>(site, true, out var torrentSite))
        {
            _logger.LogWarning("Invalid site parameter: {Site}", site);
            return BadRequest(new ErrorResponseDto
            {
                Status = "validation_error",
                Message = "Invalid site parameter. Allowed values: rutor, rutracker"
            });
        }

        var categories = torrentSite switch
        {
            TorrentSite.Rutor => RutorCategories.Categories,
            TorrentSite.Rutracker => RutrackerCategories.Categories,
            _ => new List<TorrentCategory>()
        };

        var response = new CategoriesResponseDto
        {
            Site = site.ToLowerInvariant(),
            Categories = categories
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedTorrentResponseDto>> GetTorrents(
        [FromQuery] string site,
        [FromQuery] string? category,
        [FromQuery] int page,
        CancellationToken cancellationToken)
    {
        // Validate site parameter
        if (!Enum.TryParse<TorrentSite>(site, true, out var torrentSite))
        {
            _logger.LogWarning("Invalid site parameter: {Site}", site);
            return BadRequest(new ErrorResponseDto
            {
                Status = "validation_error",
                Message = "Invalid site parameter. Allowed values: rutor, rutracker"
            });
        }

        // Validate page parameter
        if (page < 1)
        {
            _logger.LogWarning("Invalid page parameter: {Page}", page);
            return BadRequest(new ErrorResponseDto
            {
                Status = "validation_error",
                Message = "Page number must be >= 1"
            });
        }

        try
        {
            var result = await _torrentService.GetTorrentsAsync(
                torrentSite,
                category,
                page,
                cancellationToken);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching {Site} page {Page}", site, page);
            return StatusCode(503, new ErrorResponseDto
            {
                Status = "network_error",
                Message = "Unable to reach the selected torrent site. Please try again later."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {Site} page {Page}", site, page);
            return StatusCode(500, new ErrorResponseDto
            {
                Status = "parsing_error",
                Message = "We were unable to interpret the torrent listing for this page."
            });
        }
    }
}
