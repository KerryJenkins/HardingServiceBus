using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Color
{
    public class HttpServiceBus
    {
        const string SbHostName = "servicebus.windows.net";
        const string AcsHostName = "accesscontrol.windows.net";

        private readonly string _serviceNamespace;
        private readonly string _issuerName;
        private readonly string _issuerSecret;
        private readonly string _baseAddress;
        private readonly string _token;

        public HttpServiceBus(string serviceNamespace, string issuerName, string issuerSecret)
        {
            _serviceNamespace = serviceNamespace;
            _issuerName = issuerName;
            _issuerSecret = issuerSecret;
            _baseAddress = "https://" + _serviceNamespace + "." + SbHostName + "/";
            _token = GetToken(_issuerName, _issuerSecret);
        }

        private string GetToken(string issuerName, string issuerSecret)
        {
            var acsEndpoint = "https://" + _serviceNamespace + "-sb." + AcsHostName + "/WRAPv0.9/";

            // Note that the realm used when requesting a token uses the HTTP scheme, even though
            // calls to the service are always issued over HTTPS
            var realm = "http://" + _serviceNamespace + "." + SbHostName + "/";

            var values = new NameValueCollection
                             {
                                 {"wrap_name", issuerName},
                                 {"wrap_password", issuerSecret},
                                 {"wrap_scope", realm}
                             };

            var webClient = new WebClient();
            byte[] response = webClient.UploadValues(acsEndpoint, values);

            string responseString = Encoding.UTF8.GetString(response);

            var responseProperties = responseString.Split('&');
            var tokenProperty = responseProperties[0].Split('=');
            var token = Uri.UnescapeDataString(tokenProperty[1]);

            return "WRAP access_token=\"" + token + "\"";
        }

        // Uses HTTP PUT to create the queue
        public string CreateQueue(string queueName)
        {
            // Create the URI of the new Queue, note that this uses the HTTPS scheme
            string queueAddress = _baseAddress + queueName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            // Prepare the body of the create queue request
            var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                  <title type=""text"">" + queueName + @"</title>
                                  <content type=""application/xml"">
                                    <QueueDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                  </content>
                                </entry>";

            byte[] response = webClient.UploadData(queueAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        // Sends a message to the "queueName" queue, given the name, the value to enqueue, and the SWT token
        // Uses an HTTP POST request.
        public void SendMessage(string queueName, string body)
        {
            SendMessage(queueName, body, new Dictionary<string, string>());
        }

        public void SendMessage(string queueName, string body, Dictionary<string,string> headers)
        {
            string fullAddress = _baseAddress + queueName + "/messages" + "?timeout=60";
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;
            foreach (var header in headers)
            {
                webClient.Headers.Add(header.Key, header.Value);
            }

            webClient.UploadData(fullAddress, "POST", Encoding.UTF8.GetBytes(body));
        }

        // Receives and deletes the next message from the given resource (Queue, Topic, or Subscription)
        // using the resourceName, the SWT token, and an HTTP DELETE request.
        public string ReceiveAndDeleteMessage(string resourceName)
        {
            string fullAddress = _baseAddress + resourceName + "/messages/head" + "?timeout=60";
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            byte[] response = webClient.UploadData(fullAddress, "DELETE", new byte[0]);
            string responseStr = Encoding.UTF8.GetString(response);

            return responseStr;
        }

        // Creates a Topic with the given topic name and the SWT token
        // Using an HTTP PUT request.
        public string CreateTopic(string topicName)
        {
            var topicAddress = _baseAddress + topicName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            // Prepare the body of the create queue request
            var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                  <title type=""text"">" + topicName + @"</title>
                                  <content type=""application/xml"">
                                    <TopicDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                  </content>
                                </entry>";

            byte[] response = webClient.UploadData(topicAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        public string CreateSubscription(string topicName, string subscriptionName)
        {
            var subscriptionAddress = _baseAddress + topicName + "/Subscriptions/" + subscriptionName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            // Prepare the body of the create queue request
            var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                  <title type=""text"">" + subscriptionName + @"</title>
                                  <content type=""application/xml"">
                                    <SubscriptionDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                  </content>
                                </entry>";

            byte[] response = webClient.UploadData(subscriptionAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        public string CreateSubscriptionRule(string topicName, string subscriptionName, string ruleName, string rule)
        {
            var subscriptionRuleAddress = _baseAddress + topicName + "/Subscriptions/" + subscriptionName + "/rules/" + ruleName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            // Prepare the body of the create filter request
            var putData = 
            @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                <content type=""application/xml"">    
                    <RuleDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" 
                                     xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"">
                        <Filter i:type=""SqlFilter"">
                            <SqlExpression>" + rule + @"</SqlExpression>
                        </Filter>  
                    </RuleDescription>
                </content>
            </entry>";

            byte[] response = webClient.UploadData(subscriptionRuleAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        public string GetResources(string resourceAddress)
        {
            string fullAddress = _baseAddress + resourceAddress;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;
            return FormatXml(webClient.DownloadString(fullAddress));
        }

        public string DeleteResource(string resourceName)
        {
            string fullAddress = _baseAddress + resourceName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            byte[] response = webClient.UploadData(fullAddress, "DELETE", new byte[0]);
            return Encoding.UTF8.GetString(response);
        }


        public string DeleteSubscriptionRule(string topicName, string subscriptionName, string ruleName)
        {
            var subscriptionRuleAddress = _baseAddress + topicName + "/Subscriptions/" + subscriptionName + "/rules/" + ruleName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            byte[] response = webClient.UploadData(subscriptionRuleAddress, "DELETE", new byte[0]);
            return Encoding.UTF8.GetString(response);
        }
        // Formats the XML string to be more human-readable; intended for display purposes
        private static string FormatXml(string inputXml)
        {
            var document = new XmlDocument();
            document.Load(new StringReader(inputXml));

            var builder = new StringBuilder();
            using (var writer = new XmlTextWriter(new StringWriter(builder)))
            {
                writer.Formatting = Formatting.Indented;
                document.Save(writer);
            }

            return builder.ToString();
        }
    }
}
