using Apache.NMS;
using Apache.NMS.Util;
using System;
using System.Threading.Tasks;

namespace activemq_consumer
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

                        using (IConnection connection = factory.CreateConnection())
                        using (ISession session = connection.CreateSession())
                        {
                            IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                            Console.WriteLine("Using destination: " + destination);

                            // Create a consumer
                            using (IMessageConsumer consumer = session.CreateConsumer(destination))
                            {
                                // Start the connection so that messages will be processed.
                                connection.Start();

                                while (true)
                                {
                                    Console.WriteLine("Waiting for message to consume.");

                                    // Consume a message
                                    ITextMessage message = consumer.Receive() as ITextMessage;
                                    if (message == null)
                                    {
                                        Console.WriteLine("No message received!");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                                        Console.WriteLine("Received message with text: " + message.Text);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
        }
    }
}
