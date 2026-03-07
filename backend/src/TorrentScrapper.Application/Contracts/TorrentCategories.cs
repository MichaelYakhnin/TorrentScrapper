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
        new() { Id = "movies", Name = "Зарубежные фильмы", Url = "1" }, // /browse/{page}/1/0/0
        new() { Id = "russian-films", Name = "Наши фильмы", Url = "5" }, // /browse/{page}/5/0/0
        new() { Id = "foreign-series", Name = "Зарубежные сериалы", Url = "4" }, // /browse/{page}/4/0/0
        new() { Id = "russian-series", Name = "Наши сериалы", Url = "16" }, // /browse/{page}/16/0/0
        new() { Id = "tv", Name = "Телевизор", Url = "6" } // /browse/{page}/6/0/0
    };
}

public static class RutrackerCategories
{
    public static readonly List<TorrentCategory> Categories = new()
    {
        new() { Id = "foreign-films", Name = "Зарубежное кино", Url = "viewforum.php?f=7" },
        new() { Id = "russian-films", Name = "Наше кино", Url = "viewforum.php?f=22" },
        new() { Id = "foreign-series", Name = "Зарубежные сериалы", Url = "viewforum.php?f=189" },
        new() { Id = "russian-series", Name = "Наши сериалы", Url = "viewforum.php?f=842" }
    };
}
