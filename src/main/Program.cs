using service;

namespace main;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            new Program().Run().Wait();
        }
        catch (AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
        }
    }

    private async Task Run()
    {
        EmailService service = new EmailService();
        await service.GetEmails();
    }

}
