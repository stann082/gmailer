using Microsoft.Extensions.Logging;
using Serilog;
using service;
using StackExchange.Redis;

namespace ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        string baseLogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string logFilePath = Path.Combine(baseLogPath, "logs", "gmailer", "usage.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFilePath, shared: true, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
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

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost"));
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}
