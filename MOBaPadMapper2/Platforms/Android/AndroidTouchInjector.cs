using System.Threading.Tasks;

#if ANDROID
using Android.App;
using Android.Widget;
#endif

namespace MOBaPadMapper2;

public class AndroidTouchInjector : ITouchInjector
{
    public async Task TapAsync(double x, double y)
    {
#if ANDROID
        var service = TouchAccessibilityService.Instance;
        if (service == null)
        {
            // DIAGNOSTYKA: zobaczysz to jako systemowy toast
            Toast.MakeText(Android.App.Application.Context, "Touch service NOT connected", ToastLength.Short)?.Show();
            return;
        }

        await service.PerformTapAsync((float)x, (float)y);
#else
        await Task.CompletedTask;
#endif
    }

    public async Task SwipeAsync(double startX, double startY, double endX, double endY, TimeSpan duration)
    {
#if ANDROID
        var service = TouchAccessibilityService.Instance;
        if (service == null)
        {
            Toast.MakeText(Android.App.Application.Context, "Touch service NOT connected", ToastLength.Short)?.Show();
            return;
        }

        await service.PerformSwipeAsync(
            (float)startX,
            (float)startY,
            (float)endX,
            (float)endY,
            (long)duration.TotalMilliseconds);
#else
        await Task.CompletedTask;
#endif
    }
}
