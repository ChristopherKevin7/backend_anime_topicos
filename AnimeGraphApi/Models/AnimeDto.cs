namespace AnimeGraphApi.Models;

public class AnimeDto
{
    public string Title { get; set; } = string.Empty;
    public double? Score { get; set; }
    public int? Episodes { get; set; }
    public string? AnimeType { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string> Studios { get; set; } = [];
    public List<string> Producers { get; set; } = [];
    public List<string> Licensors { get; set; } = [];
    public List<StaffEntryDto> Staff { get; set; } = [];
}

public class StaffEntryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
}
