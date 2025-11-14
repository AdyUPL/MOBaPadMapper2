using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Devices;

namespace MOBaPadMapper2;

public class MobaInputMapper
{
    private readonly ITouchInjector _touch;

    /// <summary>
    /// Lista aktywnych mapowań (na razie jedna, później profile)
    /// </summary>
    public ObservableCollection<ActionMapping> Mappings { get; } = new();

    private GameProfile? _activeProfile;

    public MobaInputMapper(ITouchInjector touch)
    {
        _touch = touch;

        // 🔧 Tymczasowe, domyślne mapowanie:
        // ButtonA → tap w dolnej środkowej części ekranu (np. przycisk ataku)
        Mappings.Add(new ActionMapping
        {
            ButtonCode = "ButtonA",
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.8,
        });

        // ButtonB → tap nieco po lewej
        Mappings.Add(new ActionMapping
        {
            ButtonCode = "ButtonB",
            ActionType = ActionType.Tap,
            TargetX = 0.3,
            TargetY = 0.8,
        });

        // ButtonX → tap nieco po prawej
        Mappings.Add(new ActionMapping
        {
            ButtonCode = "ButtonX",
            ActionType = ActionType.Tap,
            TargetX = 0.7,
            TargetY = 0.8,
        });
    }

    /// <summary>
    /// Główna metoda wywoływana przy zmianie stanu przycisku pada
    /// </summary>
    public async Task HandleButtonChangedAsync(GamepadButtonEventArgs e)
    {
        // Na razie reagujemy tylko na wciśnięcie (Down), nie na puszczenie (Up)
        if (!e.IsPressed)
            return;

        // Szukamy mapowania dla tego przycisku
        var mapping = Mappings.FirstOrDefault(m => m.Matches(e));
        if (mapping == null)
            return;

        var displayInfo = DeviceDisplay.MainDisplayInfo;
        var screenWidth = displayInfo.Width;   // px
        var screenHeight = displayInfo.Height; // px

        var x = mapping.TargetX * screenWidth;
        var y = mapping.TargetY * screenHeight;

        switch (mapping.ActionType)
        {
            case ActionType.Tap:
                await _touch.TapAsync(x, y);
                break;

            case ActionType.Swipe:
                if (mapping.EndX is double ex && mapping.EndY is double ey)
                {
                    var endX = ex * screenWidth;
                    var endY = ey * screenHeight;
                    await _touch.SwipeAsync(x, y, endX, endY, mapping.Duration);
                }
                break;
        }
    }

    public void SetProfile(GameProfile profile)
    {
        _activeProfile = profile;

        // Czyścimy aktualne mapowania i wstawiamy te z profilu
        Mappings.Clear();
        foreach (var m in profile.Mappings)
        {
            Mappings.Add(m);
        }
    }
}
