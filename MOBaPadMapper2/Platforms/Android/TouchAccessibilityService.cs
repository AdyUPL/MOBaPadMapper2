#if ANDROID
using Android.AccessibilityServices;
using Android.Graphics;
using Android.Views.Accessibility;
using Android.Views;
using System.Threading.Tasks;

namespace MOBaPadMapper2;

public class TouchAccessibilityService : AccessibilityService
{
    public static TouchAccessibilityService? Instance { get; private set; }

    public override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnServiceConnected()
    {
        base.OnServiceConnected();
        Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        base.OnDestroy();
    }

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        // Nie potrzebujemy tu niczego
    }

    public override void OnInterrupt()
    {
        // Ignorujemy
    }

    public Task PerformTapAsync(float x, float y)
    {
        var path = new Android.Graphics.Path();
        path.MoveTo(x, y);

        var stroke = new GestureDescription.StrokeDescription(path, 0, 50);
        var gesture = new GestureDescription.Builder()
            .AddStroke(stroke)
            .Build();

        return DispatchGestureAsync(gesture);
    }

    public Task PerformSwipeAsync(float startX, float startY, float endX, float endY, long durationMs)
    {
        var path = new Android.Graphics.Path();
        path.MoveTo(startX, startY);
        path.LineTo(endX, endY);

        var stroke = new GestureDescription.StrokeDescription(path, 0, durationMs);
        var gesture = new GestureDescription.Builder()
            .AddStroke(stroke)
            .Build();

        return DispatchGestureAsync(gesture);
    }

    private Task DispatchGestureAsync(GestureDescription gesture)
    {
        var tcs = new TaskCompletionSource<bool>();

        bool started = DispatchGesture(gesture, new GestureCallback(tcs), null);
        if (!started)
            tcs.TrySetResult(false);

        return tcs.Task;
    }

    private class GestureCallback : GestureResultCallback
    {
        private readonly TaskCompletionSource<bool> _tcs;

        public GestureCallback(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public override void OnCompleted(GestureDescription gestureDescription)
        {
            base.OnCompleted(gestureDescription);
            _tcs.TrySetResult(true);
        }

        public override void OnCancelled(GestureDescription gestureDescription)
        {
            base.OnCancelled(gestureDescription);
            _tcs.TrySetResult(false);
        }
    }
}
#endif
