using System;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace ReceiveFromQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var client =
                QueueClient.CreateFromConnectionString(connectionString, "demo", ReceiveMode.PeekLock);

            BrokeredMessage result = null;

            try
            {
                result = client.Receive();
            
                Console.WriteLine(result.GetBody<string>());

                result.Complete();   
            }
            catch (Exception e)
            {
                result.Abandon();
                Console.WriteLine(e);
            }
            
            Console.ReadLine();
        }
    }
}
