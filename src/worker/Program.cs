using core;
using MongoDB.Driver;
using Serilog;
using service;
using worker;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Gmailer", "logs", "sync-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    await Host.CreateDefaultBuilder(args)
        .UseWindowsService(options => options.ServiceName = "Gmailer Sync")
        .ConfigureServices(services =>
        {
            services.AddSingleton<IMongoDatabase>(_ =>
                new MongoClient("mongodb://localhost:27017").GetDatabase(Constants.DatabaseName));
            services.AddSingleton<IEmailService, EmailService>();
            services.AddHostedService<SyncWorker>();
        })
        .Build()
        .RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
