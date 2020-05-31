using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace SmartConnectorTest
{
    class Program
    {
        public static async Task SendNotification(Message message, FirebaseMessaging messaging)
        {
            Console.WriteLine("message " + message);
            string response = await messaging.SendAsync(message);
            Console.WriteLine("Successfully sent message: " + response);
        }

        static async Task Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
            //Trace.TraceInformation("$message");
            //Trace.writeline(message)

        
            // Create a firebase app
            var app = FirebaseApp
                .Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(@"C:\Users\sesa525401\source\repos\SmartConnectorTest\SmartConnectorTest\serviceAccountKey.json")
                                                 .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
                });


            FirebaseMessaging messaging = FirebaseMessaging.GetMessaging(app);

            var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = "Test from server",
                        Body = "Hello "
                    },
                    Topic = "test_topic"
            };

            await SendNotification(message, messaging);

        }

    }
}
