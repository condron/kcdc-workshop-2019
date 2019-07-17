using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;

namespace Registration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //Connect to Eventstore
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting()
                .KeepRetrying()
                .UseConsoleLogger()
                .Build();

            var conn = EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();

            // Save an event 
           
            var @event = new UserRegistered(
                Guid.NewGuid(),
                "Mike",
                "Jones",
                "mike@aol.com");

            
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
            
            var eventData = new EventData(
                                    Guid.NewGuid(), 
                                    nameof(UserRegistered), 
                                    true, 
                                    data, 
                                    null);

            var stream = $"user-{@event.UserId}";
            conn.AppendToStreamAsync(stream, ExpectedVersion.NoStream, eventData).Wait();


            var registeredUsers = $"$et-{nameof(UserRegistered)}";
            var slice = conn.ReadStreamEventsForwardAsync(registeredUsers, StreamPosition.Start, 100, true).Result;
            var events = new List<UserRegistered>();
            foreach (var evt in slice.Events)

            {
                events.Add(  JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(evt.Event.Data),
                    typeof(UserRegistered))as UserRegistered);
            }

            foreach (var userRegistered in events){
                Console.WriteLine($"Registered User: {userRegistered.LastName},{userRegistered.FirstName}");
            }

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
                              string email)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
