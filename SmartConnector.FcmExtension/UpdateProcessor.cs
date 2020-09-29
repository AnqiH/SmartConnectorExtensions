using Mongoose.Common;
using Mongoose.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mongoose.Common.Attributes;
using System.Threading.Tasks;
using SxL.Common;
using Mongoose.Process.Ews;
using Mongoose.Common.Api;
using System.Threading;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace SmartConnector.FcmExtension
{
    public class UpdateProcessor : ExtensionProcessorBase, ILongRunningProcess
    {
        #region FcmTopic
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("test_app"), Tooltip("SCM Subscription topic")]
        public string FcmTopic { get; set; }
        #endregion

        #region EWS Address
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("http://localhost:81/EcoStruxure/DataExchange"), Tooltip("Default HTTP address to configure when bootstraping the EWS Server")]
        public string EwsAddress { get; set; }
        #endregion

        #region Service Account Key Path
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue(@"C:\Users\sesa525401\source\repos\SmartConnectorTest\SmartConnector.FcmExtension\serviceAccountKey.json"), Tooltip("Configure the location of the service account key")]
        public string serviceAccountKeyLocation { get; set; }
        #endregion


        private List<Prompt> prompts = new List<Prompt>();
        FirebaseMessaging messaging = null;
        FirebaseApp app;
        
        protected override IEnumerable<Prompt> Execute_Subclass()
        {

            // Make sure we can connect to an EWS Server
            if (!IsConnected) return new List<Prompt> { CreateCannotConnectPrompt() };

            var alarmReader = new AlarmItemReader
            {
                Address = this.EwsAddress,
                UserName = this.UserName,
                Password = this.Password,
                AlarmTypesFilter = new List<string>(),
            };


            if (app==null && FirebaseApp.GetInstance("[DEFAULT]") == null)
            {
                try
                {
                    app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(serviceAccountKeyLocation)
                                               .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogCategory.Processor, this.Name, $"Failed to connect to FCM service.");
                    prompts.Add(ex.ToPrompt());
                    return prompts;
                }
            }


            try
            {
                do
                {
                    readAlarms(alarmReader);
                    Thread.Sleep(10000);
                } while (!IsCancellationRequested);
            }
            catch(Exception ex)
            {
                Logger.LogError(LogCategory.Processor, "Update Processor Exception", ex.ToString());
            }

            return prompts;
        }

        void readAlarms(AlarmItemReader alarmReader)
        {
            var result = alarmReader.ReadData();
            var lastUpdated = alarmReader.LastUpdate;

            if (result.Success)
            {
                var alarmList = result.DataRead.Where(x => x.State==(Ews.Common.EwsAlarmStateEnum.Active)).OrderByDescending(x => x.Transition);

                messaging = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance("[DEFAULT]"));

                if (alarmList.Count() > 0)
                {

                    foreach (var alarm in alarmList)
                    {
                        if (!SendNotification(CreateMessage(alarm), messaging).Result)
                        {
                            prompts.Add(new Prompt { Message = "message sent unsuccessful" });
                        }
                    }
                }

            }
            else
            {
                Logger.LogDebug(LogCategory.Processor, $"Alarm read failed.");
            }
        }

        protected async Task<bool> SendNotification(Message message, FirebaseMessaging messaging)
        {
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
        

        protected Message CreateMessage(AlarmResultItem alarm)
        {
            // create notification message
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = alarm.SourceName,
                    Body = alarm.Message
                },
                Topic = FcmTopic
            };
            return message;
        }


    }
}
