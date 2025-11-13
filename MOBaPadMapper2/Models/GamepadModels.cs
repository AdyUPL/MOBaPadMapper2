namespace MOBaPadMapper2;

public enum GamepadButton
{
    A, B, X, Y,
    LB, RB, LT, RT,
    LeftStick, RightStick,
    Start, Back
}

public class StickState
{
    public float X { get; set; }   // -1..1
    public float Y { get; set; }   // -1..1
}

public class GamepadState
{
    public HashSet<GamepadButton> PressedButtons { get; set; } = new();
    public StickState LeftStick { get; set; } = new();
    public StickState RightStick { get; set; } = new();
}
