using System;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Infrastruture;
using Newtonsoft.Json;
using Registration.Blueprint.Commands;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;
using Registration.Components.CommandHandlers;
using Registration.Components.EventReaders;

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
            
            //configure read side
            var eventBus = new SimpleBus();
            var repo = new SimpleRepo(conn);
            
            var userRm = new RegisteredUsers();
            eventBus.Subscribe<UserRegistered>(userRm);
            eventBus.Subscribe<NameChanged>(userRm);

            var pump = new EventPump(conn, eventBus, repo.Deserialize);

            //configure write side
            var mainBus = new SimpleBus();

            var userSvc = new UserSvc(repo);
            mainBus.Subscribe<RegisterUser>(userSvc);
            mainBus.Subscribe<ChangeName>(userSvc);

            //application wire up
            app.CommandPublisher = mainBus;
            app.GetUsers = () => (IRegisteredUsers)userRm;
            
            //start 
            pump.Start();
        }
    }
}
