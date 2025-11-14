using System.Collections.Generic;

namespace MOBaPadMapper2;

public static class ProfilesRepository
{
    public static List<GameProfile> LoadProfiles()
    {
        // Na razie statycznie – możesz potem rozbudować o zapis do Preferences.
        var profiles = new List<GameProfile>();

        var domyslny = new GameProfile
        {
            Name = "Domyślny profil"
        };
        domyslny.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.A,
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.8,
            Size = 60
        });
        domyslny.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.B,
            ActionType = ActionType.Tap,
            TargetX = 0.8,
            TargetY = 0.8,
            Size = 60
        });

        var moba = new GameProfile
        {
            Name = "Profil MOBA"
        };
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.X,
            ActionType = ActionType.Tap,
            TargetX = 0.3,
            TargetY = 0.8,
            Size = 60
        });
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.Y,
            ActionType = ActionType.HoldAndAim,
            TargetX = 0.7,
            TargetY = 0.5,
            Size = 60
        });

        profiles.Add(domyslny);
        profiles.Add(moba);

        return profiles;
    }
}
