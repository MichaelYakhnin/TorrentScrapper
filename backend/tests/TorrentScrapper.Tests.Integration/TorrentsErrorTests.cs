using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TorrentScrapper.Application.Contracts;
using Xunit;

namespace TorrentScrapper.Tests.Integration;

public class TorrentsErrorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TorrentsErrorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("unknown")]
    [InlineData("123")]
    public async Task GetTorrents_WithInvalidSite_Returns400ValidationError(string invalidSite)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site={invalidSite}&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Status);
        Assert.Contains("Invalid site parameter", error.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetTorrents_WithInvalidPage_Returns400ValidationError(int invalidPage)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site=rutor&page={invalidPage}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Status);
        Assert.Contains("Page number must be >= 1", error.Message);
    }

    [Fact]
    public async Task GetTorrents_WithMissingSiteParameter_Returns400()
    {
        // Act
        var response = await _client.GetAsync("/api/torrents?page=1");

        // Assert  
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTorrents_WithMissingPageParameter_Returns400()
    {
        // Act
        var response = await _client.GetAsync("/api/torrents?site=rutor");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTorrents_ErrorResponse_HasCorrectStructure()
    {
        // Act - trigger validation error
        var response = await _client.GetAsync("/api/torrents?site=invalid&page=1");

        // Assert
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.NotNull(error.Status);
        Assert.NotEmpty(error.Status);
        Assert.NotNull(error.Message);
        Assert.NotEmpty(error.Message);
    }

    [Theory]
    [InlineData("rutor", 1)]
    [InlineData("rutracker", 1)]
    [InlineData("Rutor", 1)]
    [InlineData("RUTRACKER", 1)]
    public async Task GetTorrents_WithValidParameters_DoesNotReturnError(string site, int page)
    {
        // Act
        var response = await _client.GetAsync($"/api/torrents?site={site}&page={page}");

        // Assert - should be OK or possibly 503 if site is down, but not a validation error
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<PagedTorrentResponseDto>();
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task GetTorrents_CaseInsensitiveSiteParameter_Works()
    {
        // Test that site parameter is case-insensitive
        var responses = new[]
        {
            await _client.GetAsync("/api/torrents?site=rutor&page=1"),
            await _client.GetAsync("/api/torrents?site=Rutor&page=1"),
            await _client.GetAsync("/api/torrents?site=RUTOR&page=1")
        };

        // All should have same status (either OK or 503 for network issues, but not 400)
        foreach (var response in responses)
        {
            Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetTorrents_MultipleValidationErrors_ReturnsFirstError()
    {
        // Act - both invalid site and invalid page
        var response = await _client.GetAsync("/api/torrents?site=invalid&page=0");

        // Assert - should return validation error (site is validated first)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Status);
    }
}
