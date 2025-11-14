using System;
using Android.Views;

namespace MOBaPadMapper2;

public class GamepadInputService : IGamepadInputService
{
    public static GamepadInputService Instance { get; } = new();

    public event EventHandler<GamepadButtonEventArgs>? ButtonChanged;

    private GamepadInputService() { }

    public void OnKeyDown(Keycode keyCode, KeyEvent e)
    {
        ButtonChanged?.Invoke(this,
            new GamepadButtonEventArgs(keyCode.ToString(), true));
    }

    public void OnKeyUp(Keycode keyCode, KeyEvent e)
    {
        ButtonChanged?.Invoke(this,
            new GamepadButtonEventArgs(keyCode.ToString(), false));
    }

    public void OnGenericMotionEvent(MotionEvent e)
    {
        // Tu później ogarniemy analogi
    }
}
