namespace TorrentScrapper.Application.Contracts;

public class TorrentCategory
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
}

public static class RutorCategories
{
    public static readonly List<TorrentCategory> Categories = new()
    {
        new() { Id = "movies", Name = "Foreign Films", Url = "1" }, // /browse/{page}/1/0/0
        new() { Id = "russian-films", Name = "Russian Films", Url = "5" }, // /browse/{page}/5/0/0
        new() { Id = "foreign-series", Name = "Foreign Series", Url = "4" }, // /browse/{page}/4/0/0
        new() { Id = "russian-series", Name = "Russian Series", Url = "16" }, // /browse/{page}/16/0/0
        new() { Id = "tv", Name = "TV", Url = "6" } // /browse/{page}/6/0/0
    };
}

public static class RutrackerCategories
{
    public static readonly List<TorrentCategory> Categories = new()
    {
        // Updated foreign-films forum id to 252
        new() { Id = "foreign-films", Name = "Foreign Films", Url = "viewforum.php?f=252" },
        new() { Id = "russian-films", Name = "Russian Films", Url = "viewforum.php?f=22" },
        new() { Id = "foreign-series", Name = "Foreign Series", Url = "viewforum.php?f=842" },
        new() { Id = "russian-series", Name = "Russian Series", Url = "viewforum.php?f=9" }
    };
}
