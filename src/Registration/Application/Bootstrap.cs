using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Infrastructure;
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
        public static void ConfigureApp(RegistrationApp app, string eventNamespace)
        {
            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting()
                .KeepRetrying()
                //.UseConsoleLogger()
                .Build();
            var conn = EventStoreConnection.Create(settings, IPEndPoint.Parse("127.0.0.1:1113"));
            conn.ConnectAsync().Wait();


            var repo = new SimpleRepo(conn, eventNamespace);

            var userRm = new RegisteredUsers(conn, repo.Deserialize);

            var mainBus = new SimpleBus();

            var userSvc = new UserSvc(repo);
            mainBus.Subscribe<RegisterUser>(userSvc);
            mainBus.Subscribe<ChangeName>(userSvc);

            //application wire up
            app.CommandPublisher = mainBus;
            userRm.SubscribeToChanges(app.DisplayUsers);
            //start 
            userRm.Start();
            

        }
    }
}
