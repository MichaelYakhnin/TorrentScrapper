using TorrentScrapper.Application.Contracts;

namespace TorrentScrapper.Application.Abstractions;

public interface ITorrentParser
{
    Task<List<TorrentResultDto>> ParsePageAsync(int page, CancellationToken cancellationToken);
    Task<List<TorrentResultDto>> ParsePageAsync(string? categoryUrl, int page, CancellationToken cancellationToken);
}
