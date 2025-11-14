using System;

namespace MOBaPadMapper2
{
    public class GamepadButtonEventArgs : EventArgs
    {
        public string Button { get; }
        public bool IsPressed { get; }

        public GamepadButtonEventArgs(string button, bool isPressed)
        {
            Button = button;
            IsPressed = isPressed;
        }
    }
}
