using Microsoft.Extensions.Logging;
using MOBaPadMapper2;


namespace MOBaPadMapper2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<ITouchInjector, AndroidTouchInjector>();
            builder.Services.AddSingleton<IGamepadInputService>(_ => GamepadInputService.Instance);
            builder.Services.AddSingleton<MobaInputMapper>();
            builder.Services.AddSingleton<MainPage>();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
