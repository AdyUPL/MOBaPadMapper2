using Microsoft.Extensions.Logging;

namespace MOBaPadMapper2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // SERWISY
            builder.Services.AddSingleton<ITouchInjector, AndroidTouchInjector>();
            builder.Services.AddSingleton<IGamepadInputService>(_ => GamepadInputService.Instance);
            builder.Services.AddSingleton<MobaInputMapper>();

            // STRONY
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<TestPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
