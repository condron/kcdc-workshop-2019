using System;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Registration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting()
                .KeepRetrying()
                .UseConsoleLogger()
                .Build();

            var conn = EventStore.ClientAPI.EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }
    }
}
