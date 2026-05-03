namespace AnimeGraphApi.Models;

public class RelatedAnimeDto
{
    public string Title { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? AnimeType { get; set; }
    public string? SharedEntity { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? SharedEntityRole { get; set; }
}

public class PathDto
{
    public List<PathNodeDto> Nodes { get; set; } = [];
}

public class PathNodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class RelationNodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string? RelationshipRole { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? MatchedTitle { get; set; }
}
