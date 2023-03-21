using static System.Net.Mime.MediaTypeNames;

namespace Raki.TelegramBot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .ConfigureServices(services => services.AddSingleton<Executor>())
                .Build();

            var executor = host.Services.GetService<Executor>();
            await executor!.ExecuteAsync();
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            // Todo : Add dependencies here
        }
    }
}