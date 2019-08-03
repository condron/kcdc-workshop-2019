using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Registration.Application;
using Registration.Blueprint.Events;

namespace Registration
{
    class Program
    {
        static void Main(string[] args)
        {

            var app = new RegistrationApp();

            //app.PreLoadUserData();

            app.Start();
            
            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }
    }
}
