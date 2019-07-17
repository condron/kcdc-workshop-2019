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

            var conn = EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }
    }
    /*
     * UserRegistered
        * UserId: [GUID]
        * FirstName: "Mike"
        * LastName: "Jones"
        * Email: "Mike@AOL.com"
     */
    public class UserRegistered
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public UserRegistered(Guid userId,
                              string firstName,
                              string lastName,
                              string email){
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
