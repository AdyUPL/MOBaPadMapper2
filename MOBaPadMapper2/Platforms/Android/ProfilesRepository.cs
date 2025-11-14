namespace MOBaPadMapper2;

public class ProfilesRepository
{
    private readonly List<GameProfile> _profiles = new();

    public ProfilesRepository()
    {
        // Domyślny profil – możesz później go edytować
        var defaultProfile = new GameProfile
        {
            Name = "Domyślny MOBA"
        };

        defaultProfile.Mappings.AddRange(new[]
        {
            new ActionMapping
            {
                TriggerButton = GamepadButton.A,
                ActionType = ActionType.Tap,
                TargetX = 0.7,
                TargetY = 0.8,
                Size = 60
            },
            new ActionMapping
            {
                TriggerButton = GamepadButton.B,
                ActionType = ActionType.Tap,
                TargetX = 0.85,
                TargetY = 0.8,
                Size = 60
            },
            new ActionMapping
            {
                TriggerButton = GamepadButton.X,
                ActionType = ActionType.Tap,
                TargetX = 0.55,
                TargetY = 0.8,
                Size = 60
            },
            new ActionMapping
            {
                TriggerButton = GamepadButton.Y,
                ActionType = ActionType.HoldAndAim,
                TargetX = 0.7,
                TargetY = 0.6,
                UseRightStickForDirection = true,
                Size = 60
            }
        });

        _profiles.Add(defaultProfile);
    }

    public IList<GameProfile> GetAllProfiles() => _profiles;

    public GameProfile GetDefaultProfile() => _profiles.First();

    public void SaveProfile(GameProfile profile)
    {
        // Na razie w pamięci – później zapiszemy do pliku/Preferences
        var existing = _profiles.FirstOrDefault(p => p.Name == profile.Name);
        if (existing == null)
            _profiles.Add(profile);
        else
        {
            existing.Mappings = profile.Mappings;
        }
    }
}
