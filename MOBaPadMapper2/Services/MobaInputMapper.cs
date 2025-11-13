namespace MOBaPadMapper2;

public class MobaInputMapper
{
    private readonly ITouchInjector _touch;
    private readonly IList<ActionMapping> _mappings;

    private bool _isAiming;
    private ActionMapping? _currentAimMapping;
    private double _aimX;
    private double _aimY;

    public IList<ActionMapping> Mappings => _mappings;

    public MobaInputMapper(ITouchInjector touch)
    {
        _touch = touch;

        _mappings = new List<ActionMapping>
        {
            new ActionMapping
            {
                TriggerButton = GamepadButton.A,
                ActionType = ActionType.Tap,
                TargetX = 0.5,
                TargetY = 0.5
            },
            new ActionMapping
            {
                TriggerButton = GamepadButton.RB,
                ActionType = ActionType.HoldAndAim,
                TargetX = 0.8,
                TargetY = 0.8,
                UseRightStickForDirection = true
            }
        };
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

                    double length = 200;
                    double endX = _aimX + dx * length;
                    double endY = _aimY + dy * length;

                    await _touch.SwipeAsync(_aimX, _aimY, endX, endY, TimeSpan.FromMilliseconds(120));
                    _currentAimMapping = null;
                }
            }
        }
    }
}
