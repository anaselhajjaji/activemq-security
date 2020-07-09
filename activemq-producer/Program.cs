using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Transport;
using Apache.NMS.ActiveMQ.Transport.Tcp;
using Apache.NMS.ActiveMQ.Util;
using Apache.NMS.Util;
using System;
using System.Threading.Tasks;

namespace activemq_producer
{
    class Program
    {
        private const string UriString = "ssl://activemq:61617";

        static async Task Main(string[] args)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine("Waiting for 10s...");
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    try
                    {
                        Uri connecturi = new Uri(UriString);
                        Console.WriteLine("About to connect to " + connecturi);

                        SslTransportFactory ssl = new SslTransportFactory();
                        ssl.ClientCertSubject = "client";
                        ssl.ClientCertPassword = "client";
                        ssl.ClientCertFilename = "client.p12";
                        ssl.BrokerCertFilename = "activemq_cert";
                        ssl.SslProtocol = "Tls12";  //protocol, check which is using in AMQ version
                        ITransport transport = ssl.CreateTransport(connecturi);
                        
                        using (IConnection connection = new Connection(connecturi, transport, new IdGenerator()))
                        using (ISession session = connection.CreateSession())
                        {
                            IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                            Console.WriteLine("Using destination: " + destination);

                            // Create a producer
                            using (IMessageProducer producer = session.CreateProducer(destination))
                            {
                                // Start the connection so that messages will be processed.
                                connection.Start();
                                producer.DeliveryMode = MsgDeliveryMode.Persistent;

                                var count = 1;
                                while (true)
                                {
                                    // Send a message
                                    ITextMessage request = session.CreateTextMessage($"Hello World {count++}!");
                                    request.NMSCorrelationID = "abc";
                                    request.Properties["NMSXGroupID"] = "cheese";
                                    request.Properties["myHeader"] = "Cheddar";

                                    producer.Send(request);
                                    Console.WriteLine("Message sent.");

                                    Console.WriteLine("Waiting for 10s...");
                                    await Task.Delay(TimeSpan.FromSeconds(10));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
        }
    }
}