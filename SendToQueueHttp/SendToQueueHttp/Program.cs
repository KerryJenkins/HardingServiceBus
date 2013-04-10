using System;

namespace SendToQueueHttp
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpSb = new HttpServiceBus(serviceNamespace: "harding",
                                issuerName: "owner",
                                issuerSecret: "Q8Kf3NiSIzVxSAjzBpLZhYkRusN19lgbxToEi1B5toI=");

            httpSb.SendMessage("demo", "Hello Harding!");

            Console.WriteLine("Message sent!");
            Console.ReadLine();

        }
    }
}
