using Android.App;
using Android.Content.PM;
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
        GamepadInputService.Instance?.OnKeyDown(keyCode, e);
        return base.OnKeyDown(keyCode, e);
    }

    public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        GamepadInputService.Instance?.OnKeyUp(keyCode, e);
        return base.OnKeyUp(keyCode, e);
    }

    public override bool OnGenericMotionEvent(MotionEvent e)
    {
        GamepadInputService.Instance?.OnGenericMotionEvent(e);
        return base.OnGenericMotionEvent(e);
    }
}
