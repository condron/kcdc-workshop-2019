using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Infrastruture;
using Newtonsoft.Json;
using Registration.Blueprint.Commands;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;

namespace Registration.Application
{
    public class RegistrationApp{
       
        public RegistrationApp(){
            Bootstrap.ConfigureApp(this);
        }

        public IPublish CommandPublisher;
        public Func<IRegisteredUsers> GetUsers;

        public void PreLoadUserData(){
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

        private bool _stop;

        public void Start(){

            Task.Run(DisplayLoop);

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

                var changeName = new ChangeName(
                    userId,
                    "Finn",
                    "Mike"
                );

                CommandPublisher.Publish(changeName);
                input = Console.ReadLine();
            }

        }

        private void DisplayLoop(){

            while (!_stop) {
                var users = GetUsers();
                Console.Clear();
                foreach (var usersUserDisplayName in users.UserDisplayNames) {
                    Console.WriteLine(usersUserDisplayName.DisplayName);
                }
                Thread.Sleep(1000);
            }

        }

      
        public void ShutDown()
        {
            _stop = true;
        }
    }
}
