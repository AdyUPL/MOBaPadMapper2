namespace MOBaPadMapper2;

public class AndroidTouchInjector : ITouchInjector
{
    public async Task TapAsync(double x, double y)
    {
        var service = TouchAccessibilityService.Instance;
        if (service == null) return;

        await service.PerformTapAsync((float)x, (float)y);
    }

    public async Task SwipeAsync(double startX, double startY, double endX, double endY, TimeSpan duration)
    {
        var service = TouchAccessibilityService.Instance;
        if (service == null) return;

        await service.PerformSwipeAsync(
            (float)startX,
            (float)startY,
            (float)endX,
            (float)endY,
            (long)duration.TotalMilliseconds);
    }
}
