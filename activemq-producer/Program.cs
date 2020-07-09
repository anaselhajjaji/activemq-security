using Apache.NMS;
using Apache.NMS.Util;
using System;
using System.Threading.Tasks;

namespace activemq_producer
{
    class Program
    {
        private const string UriString = "activemq:tcp://activemq:61616";

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

                        // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
                        IConnectionFactory factory = new NMSConnectionFactory(connecturi);

                        using (IConnection connection = factory.CreateConnection("producer", "producer"))
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