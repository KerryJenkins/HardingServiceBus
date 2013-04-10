using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecieveFromQueueHttp
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpSb = new HttpServiceBus(serviceNamespace: "harding",
                    issuerName: "owner",
                    issuerSecret: "Q8Kf3NiSIzVxSAjzBpLZhYkRusN19lgbxToEi1B5toI=");

            var result = httpSb.ReceiveAndDeleteMessage("demo");

            Console.WriteLine(result);
            Console.ReadLine();


        }
    }
}
