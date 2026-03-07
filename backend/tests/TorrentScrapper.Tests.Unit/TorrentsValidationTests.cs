using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TorrentScrapper.Api.Controllers;
using TorrentScrapper.Application.Contracts;
using TorrentScrapper.Application.Services;
using Xunit;

namespace TorrentScrapper.Tests.Unit;

public class TorrentsValidationTests
{
    private readonly Mock<ITorrentService> _mockTorrentService;
    private readonly Mock<ILogger<TorrentsController>> _mockLogger;
    private readonly TorrentsController _controller;

    public TorrentsValidationTests()
    {
        _mockTorrentService = new Mock<ITorrentService>();
        _mockLogger = new Mock<ILogger<TorrentsController>>();
        _controller = new TorrentsController(_mockTorrentService.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("test")]
    [InlineData("INVALID")]
    public async Task GetTorrents_WithInvalidSite_ReturnsBadRequest(string invalidSite)
    {
        // Act
        var result = await _controller.GetTorrents(invalidSite, null, 1, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
        Assert.Equal("validation_error", errorResponse.Status);
        Assert.Contains("Invalid site parameter", errorResponse.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetTorrents_WithInvalidPage_ReturnsBadRequest(int invalidPage)
    {
        // Act
        var result = await _controller.GetTorrents("rutor", null, invalidPage, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
        Assert.Equal("validation_error", errorResponse.Status);
        Assert.Contains("Page number must be >= 1", errorResponse.Message);
    }

    [Theory]
    [InlineData("rutor", 1)]
    [InlineData("Rutor", 1)]
    [InlineData("RUTOR", 1)]
    [InlineData("rutracker", 1)]
    [InlineData("Rutracker", 1)]
    [InlineData("RUTRACKER", 1)]
    [InlineData("rutor", 5)]
    [InlineData("rutracker", 100)]
    public async Task GetTorrents_WithValidParameters_CallsService(string site, int page)
    {
        // Arrange
        var expectedResponse = new PagedTorrentResponseDto
        {
            Site = site.ToLowerInvariant(),
            Page = page,
            Results = new List<TorrentResultDto>()
        };

        _mockTorrentService
            .Setup(s => s.GetTorrentsAsync(It.IsAny<TorrentSite>(), It.IsAny<string>(), page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetTorrents(site, null, page, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PagedTorrentResponseDto>(okResult.Value);
        Assert.Equal(page, response.Page);
        
        _mockTorrentService.Verify(
            s => s.GetTorrentsAsync(It.IsAny<TorrentSite>(), It.IsAny<string>(), page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTorrents_WithNetworkError_ReturnsServiceUnavailable()
    {
        // Arrange
        _mockTorrentService
            .Setup(s => s.GetTorrentsAsync(It.IsAny<TorrentSite>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _controller.GetTorrents("rutor", null, 1, CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponseDto>(statusCodeResult.Value);
        Assert.Equal("network_error", errorResponse.Status);
    }

    [Fact]
    public async Task GetTorrents_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        _mockTorrentService
            .Setup(s => s.GetTorrentsAsync(It.IsAny<TorrentSite>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetTorrents("rutor", null, 1, CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponseDto>(statusCodeResult.Value);
        Assert.Equal("parsing_error", errorResponse.Status);
    }

    [Fact]
    public async Task TorrentService_WithInvalidPage_ThrowsArgumentException()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockCache = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var mockLogger = new Mock<ILogger<TorrentService>>();

        mockConfiguration
            .Setup(c => c.GetSection("Scraping:CacheExpirationMinutes").Value)
            .Returns("5");

        var service = new TorrentService(
            mockServiceProvider.Object,
            mockCache.Object,
            mockConfiguration.Object,
            mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetTorrentsAsync(TorrentSite.Rutor, 0, CancellationToken.None));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetTorrentsAsync(TorrentSite.Rutor, -1, CancellationToken.None));
    }
}
