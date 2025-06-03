namespace AvalonixAPI;

public struct SongData
{
    public string Name { get; set; }
    public string Path { get; set; }
    public int? Year { get; set; }
    public string? Author { get; set; }
    public SongData(string name, string path, int? year = null!, string? author = null!)
    {
        Name = name;
        Path = path;
        Year = year;
        Author = author;
    }
}