namespace MOBaPadMapper2;

public class GameProfile
{
    public string Name { get; set; } = string.Empty;
    public List<ActionMapping> Mappings { get; set; } = new();
}
