using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;

namespace az.sender
{
    public class Program
    {
        private static IConfigurationRoot Configuration { get; set; }
        private static EventHubClient _eventHubClient;

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder($"{Configuration["eventhub:endpoint"]}")
            {
                EntityPath = $"{Configuration["eventhub:name"]}"
            };

            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub();

            await _eventHubClient.CloseAsync();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        private static readonly Random Random = new Random();
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return (ConsoleColor)consoleColors.GetValue(Random.Next(consoleColors.Length));
        }

        private static async Task SendMessagesToEventHub()
        {
            while (true)
            {
                try
                {
                    var colour = GetRandomConsoleColor().ToString();
                    Console.WriteLine($"{colour} Selected");
                    await _eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(colour)));
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }

                await Task.Delay(10);
            }
        }
    }
}
