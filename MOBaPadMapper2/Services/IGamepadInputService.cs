using System;

namespace MOBaPadMapper2
{
    public interface IGamepadInputService
    {
        event EventHandler<GamepadButtonEventArgs>? ButtonChanged;

        void OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e);
        void OnKeyUp(Android.Views.Keycode keyCode, Android.Views.KeyEvent e);
        void OnGenericMotionEvent(Android.Views.MotionEvent e);
    }
}
