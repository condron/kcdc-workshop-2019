using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Infrastructure;
using Registration.Blueprint.Commands;
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

            var userRm = new RegisteredUsers(() => conn, repo.Deserialize);

            var mainBus = new SimpleBus();

            var userSvc = new UserSvc(repo);
            mainBus.Subscribe<RegisterUser>(userSvc);
            mainBus.Subscribe<ChangeName>(userSvc);

            //application wire up
            app.CommandPublisher = mainBus;
            userRm.Subscribe(app.DisplayUsers);
            //start 
            userRm.Start();


        }
    }
}
