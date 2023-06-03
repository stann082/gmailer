using Microsoft.Extensions.DependencyInjection;
using service;
using StackExchange.Redis;

namespace main;

public static class Program
{

    #region Main Method

    public static int Main(string[] args)
    {
        var multiplexer = ConnectionMultiplexer.Connect("localhost");
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IConnectionMultiplexer>(multiplexer)
            .AddSingleton<IEmailService, EmailService>()
            .BuildServiceProvider();
        return services.GetService<App>()!.RunApp(args);
    }

    #endregion

}
