using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;

namespace az.receiver
{
    public class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");

            var storageAccountName = $"{Configuration["storage:containername"]}";
            var storageAccountKey = $"{Configuration["storage:accountkey"]}";
            var storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            var eventProcessorHost = new EventProcessorHost(
                $"{Configuration["eventhub:name"]}",
                PartitionReceiver.DefaultConsumerGroupName,
                $"{Configuration["eventhub:endpoint"]}",
                storageConnectionString,
                $"{Configuration["storage:containername"]}");

            await eventProcessorHost.RegisterEventProcessorAsync<Processor>();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();

            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
