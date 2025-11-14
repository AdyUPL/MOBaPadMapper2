using System.Collections.ObjectModel;

namespace MOBaPadMapper2;

public class GameProfile
{
    public string Name { get; set; }

    // Konfiguracja przycisków dla danej gry
    public ObservableCollection<ActionMapping> Mappings { get; set; }

    public GameProfile()
    {
        Name = string.Empty;
        Mappings = new ObservableCollection<ActionMapping>();
    }

    public GameProfile(string name) : this()
    {
        Name = name;
    }
}
