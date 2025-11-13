using Microsoft.Extensions.DependencyInjection;
using Serilog;
using service;
using StackExchange.Redis;

namespace cli;

public static class Program
{

    #region Main Method

    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var multiplexer = await ConnectionMultiplexer.ConnectAsync("localhost");
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IConnectionMultiplexer>(multiplexer)
            .AddSingleton<IEmailService, EmailService>()
            .BuildServiceProvider();
        return await services.GetService<App>()!.RunApp(args);
    }

    #endregion

}
