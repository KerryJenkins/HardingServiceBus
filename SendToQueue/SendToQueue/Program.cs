using System;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace SendToQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var client =
                QueueClient.CreateFromConnectionString(connectionString, "demo");

            var brokeredMessage = new BrokeredMessage("Hello World!");

            client.Send(brokeredMessage);

            Console.WriteLine("Message sent to Queue.");
            Console.ReadLine();
        }
    }
}
