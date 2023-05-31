using Microsoft.Extensions.DependencyInjection;
using service;

namespace main;

public static class Program
{

    #region Main Method

    public static int Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IEmailService, EmailService>()
            .BuildServiceProvider();
        return services.GetService<App>()!.RunApp(args);
    }

    #endregion

}
