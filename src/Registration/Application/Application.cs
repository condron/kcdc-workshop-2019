using System;
using System.Collections.Generic;
using System.Text;
using EventStore.ClientAPI;
using Infrastructure;
using Newtonsoft.Json;
using Registration.Blueprint.Commands;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;

namespace Registration.Application
{
    public class RegistrationApp
    {

        public RegistrationApp()
        {
            var eventNamespace = "Registration.Blueprint.Events";
            Bootstrap.ConfigureApp(this, eventNamespace);
        }

        public IPublish CommandPublisher;

        public void PreLoadUserData()
        {
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
            // conn.AppendToStreamAsync(stream, ExpectedVersion.NoStream, eventData).Wait();


        }

        public void Start()
        {

            var input = "";
            while (input?.ToLower() != "exit") {

                //Handle Command
                var userId = Guid.NewGuid();
                var addUser = new RegisterUser(
                    userId,
                    "Billy",
                    "Bob",
                    "bob@aol.com"
                );

                CommandPublisher.Publish(addUser);
                Console.WriteLine(Environment.NewLine + "press enter to update name");
                Console.ReadLine();
                var changeName = new ChangeName(
                    userId,
                    "Finn",
                    "Bob"
                );

                CommandPublisher.Publish(changeName);
                input = Console.ReadLine();
            }
        }
        private readonly object _writeLock = new object();
        public void DisplayUsers(List<UserDisplayName> users)
        {
            lock (_writeLock) {
                Console.Clear();
                foreach (var user in users) {
                    Console.WriteLine(user.DisplayName);
                }
            }
        }
    }
}
