using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TorrentScrapper.Application.Contracts;
using Xunit;

namespace TorrentScrapper.Tests.Integration;

public class TorrentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TorrentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("rutor", 1)]
    [InlineData("rutracker", 1)]
    public async Task GetTorrents_WithValidSiteAndPage_ReturnsOk(string site, int page)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site={site}&page={page}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();
        Assert.NotNull(result);
        Assert.Equal(site, result.Site);
        Assert.Equal(page, result.Page);
        Assert.NotNull(result.Results);
    }

    [Theory]
    [InlineData("rutor", 2)]
    [InlineData("rutracker", 3)]
    public async Task GetTorrents_WithDifferentPages_ReturnsCorrectPage(string site, int page)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site={site}&page={page}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public async Task GetTorrents_WithInvalidSite_ReturnsBadRequest(string site)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site={site}&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTorrents_WithInvalidPage_ReturnsBadRequest(int page)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site=rutor&page={page}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Status);
    }

    [Fact]
    public async Task GetTorrents_Rutor_ReturnsValidTorrentData()
    {
        // Act
        var response = await _client.GetAsync("/api/torrents?site=rutor&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("rutor", result.Site);
        Assert.NotEmpty(result.Results);
        
        // Verify torrent structure
        var firstTorrent = result.Results[0];
        Assert.NotNull(firstTorrent.Name);
        Assert.NotEmpty(firstTorrent.Name);
        Assert.NotNull(firstTorrent.TorrentPageUrl);
        Assert.StartsWith("https://", firstTorrent.TorrentPageUrl);
    }

    [Fact]
    public async Task GetTorrents_Rutracker_ReturnsValidTorrentData()
    {
        // Act
        var response = await _client.GetAsync("/api/torrents?site=rutracker&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("rutracker", result.Site);
        Assert.NotEmpty(result.Results);
        
        // Verify torrent structure
        var firstTorrent = result.Results[0];
        Assert.NotNull(firstTorrent.Name);
        Assert.NotEmpty(firstTorrent.Name);
        Assert.NotNull(firstTorrent.TorrentPageUrl);
        Assert.StartsWith("https://", firstTorrent.TorrentPageUrl);
    }

    [Fact]
    public async Task GetTorrents_CachingBehavior_ReturnsSameResultsForSamePage()
    {
        // Act - First request
        var response1 = await _client.GetAsync("/api/torrents?site=rutor&page=1");
        var result1 = await response1.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();

        // Act - Second request (should be cached)
        var response2 = await _client.GetAsync("/api/torrents?site=rutor&page=1");
        var result2 = await response2.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Results.Count, result2.Results.Count);
    }
}
