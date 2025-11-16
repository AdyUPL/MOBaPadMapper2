using System.Collections.Generic;

namespace MOBaPadMapper2;

public  class ProfilesRepository
{
    public static List<GameProfile> LoadProfiles()
    {
        var profiles = new List<GameProfile>();

        // ✅ Domyślny profil – prosty
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
            TargetX = 0.3,
            TargetY = 0.8,
            Size = 60
        });

        domyslny.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.X,
            ActionType = ActionType.Tap,
            TargetX = 0.7,
            TargetY = 0.8,
            Size = 60
        });

        // ✅ Profil MOBA – przykład
        var moba = new GameProfile
        {
            Name = "MOBA – klasyczny"
        };

        // Atak podstawowy
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.A,
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.85,
            Size = 60
        });

        // Umiejętność 1
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.X,
            ActionType = ActionType.Tap,
            TargetX = 0.7,
            TargetY = 0.7,
            Size = 60
        });

        // Umiejętność 2
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.B,
            ActionType = ActionType.Tap,
            TargetX = 0.3,
            TargetY = 0.7,
            Size = 60
        });

        // Ult – pod „HoldAndAim”, na razie tylko jako info
        moba.Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.Y,
            ActionType = ActionType.HoldAndAim,
            TargetX = 0.5,
            TargetY = 0.5,
            Size = 60,
            UseRightStickForDirection = true
        });

        profiles.Add(domyslny);
        profiles.Add(moba);

        return profiles;
    }
}
