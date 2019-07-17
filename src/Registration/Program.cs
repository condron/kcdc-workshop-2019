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
            
          /*  var registered = new UserRegistered(
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
            */

            //create User Service

            var userSvc = new UserSvc(conn);


            //create readmodel

            Program.usersRm = new RegisteredUsersRM();

            //populate ReadModel
            var registeredUsers = "$ce-user";
            // var slice = conn.ReadStreamEventsForwardAsync(registeredUsers, StreamPosition.Start, 100, true).Result;
            var sub = conn.SubscribeToStreamFrom(registeredUsers,
                null,
                CatchUpSubscriptionSettings.Default,
                gotUserEvent,
                liveStarted);




            //Handle Command
            var userId = Guid.NewGuid();
            var addUser = new RegisterUser(
                userId,
                "Billy",
                "Bob",
                "bob@aol.com"
            );

            userSvc.Handle(addUser);

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }

        private static void liveStarted(EventStoreCatchUpSubscription obj)
        {
            //Update display
            foreach (var user in Program.usersRm.Users.Values)
            {
                Console.WriteLine($"Registered User: {user.FullName}");
            }

            live = true;
        }

        private static RegisteredUsersRM usersRm;
        private static bool live;
        private static Task gotUserEvent(EventStoreCatchUpSubscription sub, ResolvedEvent evt)
        {

            if (evt.Event.EventType == nameof(UserRegistered))
            {
                var userEvent = JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(evt.Event.Data),
                    typeof(UserRegistered)) as UserRegistered;
                usersRm.Handle(userEvent);
                if (live)
                {
                    Console.WriteLine($"Registered User: {usersRm.Users[userEvent.UserId].FullName}");
                }
            }
            if (evt.Event.EventType == nameof(NameChanged))
            {
                var userEvent = JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(evt.Event.Data),
                    typeof(NameChanged)) as NameChanged;
                usersRm.Handle(userEvent);
                if (live)
                {
                    Console.WriteLine($"Registered User: {usersRm.Users[userEvent.UserId].FullName}");
                }

            }



            return Task.CompletedTask;
        }
    }
    #region infrastructure
    public interface IMessage { }
    public interface IEvent : IMessage { }

    public interface IHandle<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }
    public interface ICommand : IMessage { }
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        bool Handle(TCommand cmd);
    }
    #endregion

    #region Read model

    public class RegisteredUsersRM :
        IHandle<UserRegistered>,
        IHandle<NameChanged>
    {
        public readonly Dictionary<Guid, User> Users = new Dictionary<Guid, User>();

        public void Handle(UserRegistered @event)
        {
            try
            {
                Users.Add(@event.UserId, new User(@event.FirstName, @event.LastName));
            }
            catch (Exception e)
            {
                //  Console.WriteLine(e);
            }


        }
        public void Handle(NameChanged @event)
        {
            var user = Users[@event.UserId];
            user.Firstname = @event.FirstName;
            user.Lastname = @event.LastName;
        }
        public class User
        {
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string FullName => $"{Lastname}, {Firstname}";

            public User(
                string firstname,
                string lastname)
            {
                Firstname = firstname;
                Lastname = lastname;
            }
        }
    }
    #endregion



    #region Application Service

    public class UserSvc :
        IHandleCommand<RegisterUser>
    {
        private readonly IEventStoreConnection _conn;

        public UserSvc(IEventStoreConnection conn)
        {
            _conn = conn;
        }
        public bool Handle(RegisterUser cmd)
        {
            try
            {
                  var user = new user(
                      cmd.UserId,
                      cmd.FirstName,
                      cmd.LastName,
                      cmd.Email);
                  Save(user,_conn);
            }
            catch (Exception _)//todo:try harder
            {
                return false;
            }
            return true;
        }

        //move to infra
        private static void Save(user user, IEventStoreConnection conn)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            var eventData = new List<EventData>();
            foreach (var @event in user.TakEvents())
            {
                var dString = JsonConvert.SerializeObject(@event, serializerSettings);
                var data = Encoding.UTF8.GetBytes(dString);
                var typeName = @event.GetType().Name;
                eventData.Add(new EventData(Guid.NewGuid(), typeName, true, data, null));
            }
            var stream = $"{nameof(Registration.user)}-{user.Id:N}";
            //todo: add correct expected version
            conn.AppendToStreamAsync(stream, ExpectedVersion.Any, eventData).Wait();
        }
        private static T Load<T>(Guid id, IEventStoreConnection conn) where T : user
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var stream = $"{typeof(T).Name}-{id:N}";
            var slice = conn.ReadStreamEventsForwardAsync(stream, StreamPosition.Start, 100, true).Result;
            var events = new List<IEvent>();
            foreach (var @event in slice.Events)

            {
                events.Add(JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(@event.Event.Data),
                    Type.GetType((string)@event.Event.EventType),
                    serializerSettings) as IEvent);
            }

            var agg = (T)Activator.CreateInstance(typeof(T), true);
            //todo add base class
            ((user)agg).Hydrate(events);
            return agg;
        }
    }
    #endregion
    #region Aggregate
    public class user
    {

        public user(
            Guid id,
            string firstName,
            string lastname,
            string email)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastname))
                throw new Exception("User must have a name");
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("User must have a name");
            }

            if (id == Guid.Empty)
                throw new Exception("User must have a non empty id");

            Raise(new UserRegistered(id, firstName, lastname, email));
        }
        public void ChangeName(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new Exception("User must have a name");
            Raise(new NameChanged(Id, first, last));
        }
        //state changes
        void Apply(UserRegistered @event)
        {
            Id = @event.UserId;
        }

        void Apply(NameChanged @event)
        {
            // nothing to do
        }

        //infrastructure
        private long _version;
        public Guid Id { get; private set; }
        //for hydration via infra.
        internal user() { }

        internal void Hydrate(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                dynamic evt = @event;
                Apply(evt);
                _version++;
            }
        }
        private void Raise(IEvent @event)
        {
            _pendingEvents.Add(@event);
            
            _version++;
            dynamic evt = @event;
            Apply(evt);
        }


        public IReadOnlyList<IEvent> TakEvents()
        {
            var pending = new List<IEvent>(_pendingEvents);
            _pendingEvents.Clear();
            return pending;
        }
        private readonly List<IEvent> _pendingEvents = new List<IEvent>();


    }
    #endregion
    #region messages
    /*
     * UserRegistered
        * UserId: [GUID]
        * FirstName: "Mike"
        * LastName: "Jones"
        * Email: "Mike@AOL.com"
     */
    public class RegisterUser : ICommand
    {
        public readonly Guid UserId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public RegisterUser(Guid userId,
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
    public class UserRegistered : IEvent
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
    public class NameChanged : IEvent
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
    #endregion
}
