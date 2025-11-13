#if ANDROID
using Android.AccessibilityServices;
using Android.Graphics;
using Android.OS;
using Android.Views.Accessibility;
using Android.Views;
using System.Threading.Tasks;

namespace MOBaPadMapper2
{
    public class TouchAccessibilityService : AccessibilityService
    {
        // Singleton używany przez AndroidTouchInjector
        public static TouchAccessibilityService? Instance { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
                Instance = null;
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Instance = this;
        }

        public override void OnAccessibilityEvent(AccessibilityEvent? e)
        {
            // Nie potrzebujemy reagować na eventy – tylko wstrzykujemy gesty
        }

        public override void OnInterrupt()
        {
            // tu nic nie musi być
        }

        // --- PUBLIC API używany przez AndroidTouchInjector ---

        public Task PerformTapAsync(float x, float y)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();

            var path = new Android.Graphics.Path();
            path.MoveTo(x, y);

            var stroke = new GestureDescription.StrokeDescription(path, 0, 80); // 80ms tap
            var gesture = new GestureDescription.Builder()
                .AddStroke(stroke)
                .Build();

            DispatchGesture(gesture, new GestureCallback(tcs), null);

            return tcs.Task;
        }

        public Task PerformSwipeAsync(float startX, float startY, float endX, float endY, long durationMs)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();

            var path = new Android.Graphics.Path();
            path.MoveTo(startX, startY);
            path.LineTo(endX, endY);

            if (durationMs <= 0)
                durationMs = 150;

            var stroke = new GestureDescription.StrokeDescription(path, 0, durationMs);
            var gesture = new GestureDescription.Builder()
                .AddStroke(stroke)
                .Build();

            DispatchGesture(gesture, new GestureCallback(tcs), null);

            return tcs.Task;
        }

        // Pomocnicza klasa do mapowania callbacku Androida na Task
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
}
#endif
