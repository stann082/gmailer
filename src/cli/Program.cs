using core;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using service;

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

        var services = new ServiceCollection()
            .AddSingleton<App>()
            .AddSingleton<IMongoDatabase>(_ => new MongoClient("mongodb://localhost:27017").GetDatabase(Constants.DatabaseName))
            .AddSingleton<IEmailService, EmailService>()
            .BuildServiceProvider();
        return await services.GetService<App>()!.RunApp(args);
    }

    #endregion

}
