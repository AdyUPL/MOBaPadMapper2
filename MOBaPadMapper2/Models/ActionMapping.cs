namespace MOBaPadMapper2;

public enum ActionType
{
    Tap,
    Swipe,
    HoldAndAim
}

public class ActionMapping
{
    public GamepadButton TriggerButton { get; set; }
    public ActionType ActionType { get; set; }

    // współrzędne w procentach ekranu (0..1)
    public double TargetX { get; set; }
    public double TargetY { get; set; }

    public double TargetX2 { get; set; }
    public double TargetY2 { get; set; }

    public bool UseRightStickForDirection { get; set; }

    // ROZMIAR PRZYCISKU NA EKRANIE KONFIGURACJI
    public double Size { get; set; } = 60;
}
