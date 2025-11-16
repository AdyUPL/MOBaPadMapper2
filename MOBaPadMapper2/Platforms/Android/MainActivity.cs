using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui;

namespace MOBaPadMapper2;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        GamepadInputService.Instance.OnKeyDown(keyCode, e);

        // 👉 Jeśli to zdarzenie pochodzi z gamepada / joysticka – NIE przekazujemy do bazowej implementacji,
        // żeby np. ButtonB nie działał jak systemowy "Back".
        if (IsGamepadEvent(e))
            return true;

        return base.OnKeyDown(keyCode, e);
    }

    public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        GamepadInputService.Instance.OnKeyUp(keyCode, e);

        if (IsGamepadEvent(e))
            return true;

        return base.OnKeyUp(keyCode, e);
    }

    public override bool OnGenericMotionEvent(MotionEvent e)
    {
        GamepadInputService.Instance.OnGenericMotionEvent(e);

        if (IsGamepadSource(e.Source))
            return true;

        return base.OnGenericMotionEvent(e);
    }

    private bool IsGamepadEvent(KeyEvent e)
    {
        return IsGamepadSource(e.Source);
    }

    private bool IsGamepadSource(InputSourceType source)
    {
        // Gamepad lub Joystick – to nasze
        var isGamepad =
            (source & InputSourceType.Gamepad) == InputSourceType.Gamepad ||
            (source & InputSourceType.Joystick) == InputSourceType.Joystick;

        return isGamepad;
    }
}
