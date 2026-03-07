namespace TorrentScrapper.Application.Contracts;

public class TorrentResultDto
{
    public required string Name { get; set; }
    public required string TorrentPageUrl { get; set; }
    public string? ImageUrl { get; set; }
}

public class PagedTorrentResponseDto
{
    public required string Site { get; set; }
    public required int Page { get; set; }
    public required List<TorrentResultDto> Results { get; set; }
}

public class CategoriesResponseDto
{
    public required string Site { get; set; }
    public required List<TorrentCategory> Categories { get; set; }
}

public class ErrorResponseDto
{
    public required string Status { get; set; }
    public required string Message { get; set; }
    public string? Details { get; set; }
}
