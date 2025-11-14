using System;

namespace MOBaPadMapper2;

public interface ITouchInjector
{
    Task TapAsync(double x, double y);
    Task SwipeAsync(double startX, double startY, double endX, double endY, TimeSpan duration);
}

public interface IGamepadInputService
{
    // Prosty event: jaki przycisk, czy wciœniêty / puszczony
    event EventHandler<GamepadButtonEventArgs>? ButtonChanged;
}
