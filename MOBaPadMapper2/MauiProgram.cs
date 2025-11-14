using Microsoft.Extensions.Logging;

namespace MOBaPadMapper2;

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

#if ANDROID
        builder.Services.AddSingleton<ITouchInjector, AndroidTouchInjector>();
#endif

        // Gamepad
        builder.Services.AddSingleton<IGamepadInputService>(_ => GamepadInputService.Instance);

        // Mapper (na razie prosty, ale potrzebny do DI)
        builder.Services.AddSingleton<MobaInputMapper>();

        // Strony
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MobaInputMapper>();
        builder.Services.AddSingleton<ProfilesRepository>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
