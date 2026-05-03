namespace AnimeGraphApi.Models;

public class CharacterDto
{
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
}

public class VoiceActorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Language { get; set; }
}

public class RelatedCharacterDto
{
    public string CharacterName { get; set; } = string.Empty;
    public string AnimeTitle { get; set; } = string.Empty;
    public string? Role { get; set; }
    public List<VoiceActorDto> VoiceActors { get; set; } = [];
}
