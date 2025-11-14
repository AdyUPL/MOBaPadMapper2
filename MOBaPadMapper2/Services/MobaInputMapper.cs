using Microsoft.Maui.Storage;
using System.Text.Json;

namespace MOBaPadMapper2;

public class MobaInputMapper
{
    private readonly ITouchInjector _touch;
    private readonly List<ActionMapping> _mappings;

    private bool _isAiming;
    private ActionMapping? _currentAimMapping;
    private double _aimX;
    private double _aimY;

    private const string StorageKey = "MobaInputMappings_v1";

    // UJAWNIENIE MAPOWAÑ DLA UI
    public IList<ActionMapping> Mappings => _mappings;

    public MobaInputMapper(ITouchInjector touch)
    {
        _touch = touch;
        _mappings = new List<ActionMapping>();
        LoadMappings();
    }

    /// <summary>
    /// Podmieniamy mapowania z UI i od razu zapisujemy.
    /// </summary>
    public void UpdateMappings(IEnumerable<ActionMapping> mappings)
    {
        _mappings.Clear();
        _mappings.AddRange(mappings);
        SaveMappings();
    }

    public void SaveMappings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_mappings);
            Preferences.Set(StorageKey, json);
        }
        catch
        {
            // ignorujemy b³¹d zapisu
        }
    }

    private void LoadMappings()
    {
        try
        {
            if (Preferences.ContainsKey(StorageKey))
            {
                var json = Preferences.Get(StorageKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<List<ActionMapping>>(json);
                    if (loaded != null && loaded.Count > 0)
                    {
                        _mappings.AddRange(loaded);
                        return;
                    }
                }
            }
        }
        catch
        {
            // w razie b³êdu – leæmy na domyœlnych
        }

        // DOMYŒLNE MAPOWANIA (przyk³ad)
        _mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.A,
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.5,
            Size = 60
        });

        _mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.RB,
            ActionType = ActionType.HoldAndAim,
            TargetX = 0.8,
            TargetY = 0.8,
            UseRightStickForDirection = true,
            Size = 60
        });
    }

    public async Task OnGamepadStateChanged(GamepadState state, double screenWidth, double screenHeight)
    {
        foreach (var mapping in _mappings)
        {
            bool pressed = state.PressedButtons.Contains(mapping.TriggerButton);

            if (mapping.ActionType == ActionType.Tap && pressed)
            {
                double x = mapping.TargetX * screenWidth;
                double y = mapping.TargetY * screenHeight;
                await _touch.TapAsync(x, y);
            }
            else if (mapping.ActionType == ActionType.Swipe && pressed)
            {
                double x1 = mapping.TargetX * screenWidth;
                double y1 = mapping.TargetY * screenHeight;
                double x2 = mapping.TargetX2 * screenWidth;
                double y2 = mapping.TargetY2 * screenHeight;
                await _touch.SwipeAsync(x1, y1, x2, y2, TimeSpan.FromMilliseconds(150));
            }
            else if (mapping.ActionType == ActionType.HoldAndAim)
            {
                if (pressed && !_isAiming)
                {
                    _isAiming = true;
                    _currentAimMapping = mapping;
                    _aimX = mapping.TargetX * screenWidth;
                    _aimY = mapping.TargetY * screenHeight;
                }
                else if (!pressed && _isAiming && _currentAimMapping == mapping)
                {
                    _isAiming = false;

                    var dx = state.RightStick.X;
                    var dy = -state.RightStick.Y;

                    double length = 200; // px – d³ugoœæ swipe
                    double endX = _aimX + dx * length;
                    double endY = _aimY + dy * length;

                    await _touch.SwipeAsync(_aimX, _aimY, endX, endY, TimeSpan.FromMilliseconds(120));
                    _currentAimMapping = null;
                }
            }
        }
    }
}
