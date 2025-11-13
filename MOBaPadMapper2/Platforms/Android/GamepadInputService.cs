using Android.Views;

namespace MOBaPadMapper2;

public class GamepadInputService : IGamepadInputService
{
    public static GamepadInputService Instance { get; } = new();

    private readonly GamepadState _state = new();

    public event EventHandler<GamepadState>? GamepadUpdated;

    public void OnKeyDown(Keycode keyCode, KeyEvent e)
    {
        if (IsGamepadKey(keyCode, out var button))
        {
            _state.PressedButtons.Add(button);
            GamepadUpdated?.Invoke(this, CloneState());
        }
    }

    public void OnKeyUp(Keycode keyCode, KeyEvent e)
    {
        if (IsGamepadKey(keyCode, out var button))
        {
            _state.PressedButtons.Remove(button);
            GamepadUpdated?.Invoke(this, CloneState());
        }
    }

    public void OnGenericMotionEvent(MotionEvent e)
    {
        _state.RightStick.X = e.GetAxisValue(Axis.Rx);
        _state.RightStick.Y = e.GetAxisValue(Axis.Ry);
        _state.LeftStick.X = e.GetAxisValue(Axis.X);
        _state.LeftStick.Y = e.GetAxisValue(Axis.Y);

        GamepadUpdated?.Invoke(this, CloneState());
    }

    private bool IsGamepadKey(Keycode keyCode, out GamepadButton button)
    {
        button = default;
        switch (keyCode)
        {
            case Keycode.ButtonA: button = GamepadButton.A; return true;
            case Keycode.ButtonB: button = GamepadButton.B; return true;
            case Keycode.ButtonX: button = GamepadButton.X; return true;
            case Keycode.ButtonY: button = GamepadButton.Y; return true;
            case Keycode.ButtonL1: button = GamepadButton.LB; return true;
            case Keycode.ButtonR1: button = GamepadButton.RB; return true;
            default: return false;
        }
    }

    private GamepadState CloneState()
    {
        return new GamepadState
        {
            LeftStick = new StickState { X = _state.LeftStick.X, Y = _state.LeftStick.Y },
            RightStick = new StickState { X = _state.RightStick.X, Y = _state.RightStick.Y },
            PressedButtons = new HashSet<GamepadButton>(_state.PressedButtons)
        };
    }
}
