using System;

namespace MOBaPadMapper2;

public enum ActionType
{
    Tap,
    Swipe,
    HoldAndAim
}

public enum GamepadButton
{
    A, B, X, Y,
    LB, RB, LT, RT,
    LeftStick, RightStick,
    DpadUp, DpadDown, DpadLeft, DpadRight,
    Start, Back
}

public class ActionMapping
{
    public GamepadButton? TriggerButton { get; set; }

    public string ButtonCode { get; set; } = string.Empty;

    public ActionType ActionType { get; set; } = ActionType.Tap;

    public double TargetX { get; set; }
    public double TargetY { get; set; }

    public double Size { get; set; } = 60;

    // 🔹 NOWE POLE – używane przez ProfilesRepository
    /// <summary>
    /// Jeśli true – kierunek ataku/umiejętności pobierany z prawej gałki.
    /// Na razie może być niewykorzystane w runtime, ważne żeby model je miał.
    /// </summary>
    public bool UseRightStickForDirection { get; set; } = false;

    public double? EndX { get; set; }
    public double? EndY { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(100);

    //public bool Matches(GamepadButtonEventArgs e)
    //{
    //    if (!string.IsNullOrEmpty(ButtonCode) &&
    //        ButtonCode.Equals(e.Button, StringComparison.OrdinalIgnoreCase))
    //        return true;

    //    if (TriggerButton.HasValue &&
    //        e.Button.Equals(TriggerButton.Value.ToString(), StringComparison.OrdinalIgnoreCase))
    //        return true;

    //    return false;
    //}
    public bool Matches(GamepadButtonEventArgs e)
    {
        var buttonStr = (e.Button ?? "").ToLowerInvariant();

        if (!string.IsNullOrEmpty(ButtonCode) &&
            buttonStr.Contains(ButtonCode.ToLowerInvariant()))
            return true;

        if (TriggerButton.HasValue)
        {
            var name = TriggerButton.Value.ToString().ToLowerInvariant(); // np. "A" -> "a"
            if (buttonStr == name || buttonStr.EndsWith(name))
                return true;
        }

        return false;
    }

}
