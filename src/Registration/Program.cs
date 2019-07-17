using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                //.UseConsoleLogger()
                .Build();

            var conn = EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();

            // Save an event 
            var userId = Guid.NewGuid();
            var registered = new UserRegistered(
                userId,
                "Mike",
                "Jones",
                "mike@aol.com");

            var namechange = new NameChanged(
                userId,
                "Micheal",
                "Jones");


            var eventData = new[]{
                new EventData(
                    Guid.NewGuid(),
                    nameof(UserRegistered),
                    true,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(registered)),
                    null),
                new EventData(
                    Guid.NewGuid(),
                    nameof(NameChanged),
                    true,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(namechange)),
                    null),
            };

            var stream = $"user-{userId}";
            conn.AppendToStreamAsync(stream, ExpectedVersion.NoStream, eventData).Wait();
            //create readmodel

            Program.usersRm = new RegisteredUsersRM();

            //populate ReadModel
            var registeredUsers = "$ce-user";
           // var slice = conn.ReadStreamEventsForwardAsync(registeredUsers, StreamPosition.Start, 100, true).Result;
           var sub = conn.SubscribeToStreamFrom(registeredUsers,
               null, CatchUpSubscriptionSettings.Default, 
               gotUserEvent,
               liveStarted);

            
           

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }

        private static void liveStarted(EventStoreCatchUpSubscription obj){
            //Update display
            foreach (var user in Program.usersRm.Users.Values){
                Console.WriteLine($"Registered User: {user.FullName}");
            }

            live = true;
        }

        private static RegisteredUsersRM usersRm;
        private static bool live;
        private static Task gotUserEvent(EventStoreCatchUpSubscription sub, ResolvedEvent evt)
        {
           
            if (evt.Event.EventType == nameof(UserRegistered)){
                var userEvent = JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(evt.Event.Data),
                    typeof(UserRegistered)) as UserRegistered;
                usersRm.Handle(userEvent);
                if (live){
                    Console.WriteLine($"Registered User: {usersRm.Users[userEvent.UserId].FullName}");
                }
            }
            if (evt.Event.EventType == nameof(NameChanged)){
                var userEvent = JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(evt.Event.Data),
                    typeof(NameChanged)) as NameChanged;
                usersRm.Handle(userEvent);
                if (live){
                    Console.WriteLine($"Registered User: {usersRm.Users[userEvent.UserId].FullName}");
                }

            }

           

            return Task.CompletedTask;
        }
    }
    public interface IMessage { }
    public interface IEvent:IMessage { }

    public interface IHandle<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }
    public class RegisteredUsersRM: 
        IHandle<UserRegistered>,
        IHandle<NameChanged>
    {
        public readonly Dictionary<Guid,User> Users = new Dictionary<Guid, User>();

        public void Handle(UserRegistered @event){
            try{
                Users.Add(@event.UserId , new User(@event.FirstName,@event.LastName));
            }
            catch (Exception e){
              //  Console.WriteLine(e);
            }
           

        }
        public void Handle(NameChanged @event){
            var user = Users[@event.UserId];
            user.Firstname = @event.FirstName;
            user.Lastname = @event.LastName;
        }
        public class User{
            public string Firstname{ get; set; }
            public string Lastname{ get; set; }
            public string FullName => $"{Lastname}, {Firstname}";

            public User(
                string firstname,
                string lastname){
                Firstname = firstname;
                Lastname = lastname;
            }
        }

      
    }

    /*
     * UserRegistered
        * UserId: [GUID]
        * FirstName: "Mike"
        * LastName: "Jones"
        * Email: "Mike@AOL.com"
     */
    public class UserRegistered:IEvent
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
    public class NameChanged:IEvent
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;

        public NameChanged(Guid userId,
            string firstName,
            string lastName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
