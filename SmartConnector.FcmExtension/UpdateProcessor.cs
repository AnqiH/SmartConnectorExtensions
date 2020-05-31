using Mongoose.Common;
using Mongoose.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using FirebaseAdmin;
//using FirebaseAdmin.Messaging;
//using Google.Apis.Auth.OAuth2;
using SxL.Common;

namespace SmartConnector.FcmExtension
{
    public class UpdateProcessor : ExtensionProcessorBase
    {

        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            var prompts = new List<Prompt>();

            // Make sure we can connect to an EWS Server
            if (!IsConnected) return new List<Prompt> { CreateCannotConnectPrompt() };

            /*
            FirebaseMessaging messaging;
            // Set up FCM connection
            try
            {
                // Create a firebase app
                var app = FirebaseApp
                    .Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(@"C:\Users\sesa525401\source\repos\SmartConnectorTest\SmartConnectorTest\serviceAccountKey.json")
                                                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
                    });

                messaging = FirebaseMessaging.GetMessaging(app);
            }
            catch(Exception ex)
            {
                Logger.LogError(LogCategory.Processor, this.Name, $"Failed to connect to IoT Hub.");
                prompts.Add(ex.ToPrompt());
                return prompts;
            }


            // Set up alarm reader


            // push new alarms to FCM
            try
            {
                do
                {
                    // Compose message and send to FCm

                    var message = new Message()
                    {
                        Notification = new Notification()
                        {
                            Title = "Test from server",
                            Body = "Hello "
                        },
                        Topic = "test_topic"
                    };

                    if (!SendNotification(message, messaging).Result)
                    {
                        prompts.Add(new Prompt { Message = "message sent unsuccessful" });
                    }

                } while (!!IsCancellationRequested);
            }catch(Exception ex)
            {
                prompts.Add(ex.ToPrompt());
                return prompts;
            }
            */
            return prompts;
        }
        /*
        protected async Task<bool> SendNotification(Message message, FirebaseMessaging messaging)
        {
            Logger.LogInfo(LogCategory.Processor, "sendNotif", $"message {message}");
            try
            {
                string response = await messaging.SendAsync(message);
                Logger.LogInfo(LogCategory.Processor, "sendNotif", $"Seccessfully send message: {response}");
            }catch(Exception ex)
            {
                Logger.LogError(LogCategory.Processor, "sendNotif", ex.ToString());
                return false;
            }
            return true;
        }
        */

    }
}
