using Microsoft.Extensions.Logging;
using service;
using StackExchange.Redis;
using UraniumUI;

namespace ui;

public static class MauiProgram
{

    #region Public Methods

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var multiplexer = ConnectionMultiplexer.Connect("localhost");
        builder.Services
            .AddSingleton<MainPage>()
            .AddSingleton<IConnectionMultiplexer>(multiplexer)
            .AddSingleton<IEmailService, EmailService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    #endregion

}
