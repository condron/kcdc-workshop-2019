using System;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Registration.Blueprint.Commands;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;
using Registration.CommandHandlers;
using Registration.EventReaders;
using Registration.infrastructure;

namespace Registration.Application
{
    public static class Bootstrap
    {



        public static void ConfigureApp(RegistrationApp app)
        {
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting()
                .KeepRetrying()
                //.UseConsoleLogger()
                .Build();
            var conn = EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();

            var mainBus = new SimpleBus();
            var eventBus = new SimpleBus();

            var repo = new SimpleRepo(conn);
            var pump = new EventPump(conn, eventBus, repo.Deserialize);

            var userRm = new RegisteredUsers();
            eventBus.Subscribe<UserRegistered>(userRm);
            eventBus.Subscribe<NameChanged>(userRm);

            var userSvc = new UserSvc(repo);
            mainBus.Subscribe<RegisterUser>(userSvc);
            mainBus.Subscribe<ChangeName>(userSvc);

            pump.Start();

            app.CommandPublisher = mainBus;
            app.GetUsers = () => (IRegisteredUsers)userRm;
        }
    }
}
