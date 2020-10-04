using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Ews.Common;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Ews.Server.Data;
using Mongoose.Ews.Server.Data.Shared;
using Mongoose.Process;
using Mongoose.Process.Ews;
using SxL.Common;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace SmartConnector.AlarmExtension
{
    [ConfigurationDefaults("Alarm Notification Update Processor", "Creates EWS an servers/data adapter, and retrieves new alarms from AlarmReader")]
    public class AlarmUpdateProcessor : ExtensionProcessorBase, ILongRunningProcess
    {

        #region EwsAddress
        /// <summary>
        /// The address for EWS server
        /// </summary>
        [Required, DefaultValue("http://localhost:1900/SmartConnectorAlarmService"), Tooltip("Default HTTP address to configure when bootstraping EWS Server")]
        public string EwsAddress { get; set; }
        #endregion

        #region ES Address
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("http://localhost:81/EcoStruxure/DataExchange"), Tooltip("HTTP address of the ES")]
        public string EsAddress { get; set; }
        #endregion

        #region Service Account Key Path
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue(@"C:\Users\sesa525401\source\repos\SmartConnectorTest\SmartConnector.FcmExtension\serviceAccountKey.json"), Tooltip("Configure the location of the service account key")]
        public string serviceAccountKeyLocation { get; set; }
        #endregion

        #region FcmTopic
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("test_app"), Tooltip("SCM Subscription topic")]
        public string FcmTopic { get; set; }
        #endregion

        private List<Prompt> prompts = new List<Prompt>();
        FirebaseMessaging messaging = null;
        FirebaseApp app;

        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            // Make sure to connect to a EWS server
            if (!IsConnected) return new List<Prompt> { CreateCannotConnectPrompt() };
            //throw new NotImplementedException();
            if (!DataAdapter.Server.IsRunning) DataAdapter.StartServer();

            EnsureServerParameters();

            var alarmReader = new AlarmItemReader
            {
                Address = this.EsAddress,
                UserName = this.UserName,
                Password = this.Password,
                AlarmTypesFilter = new List<string>(),
            };


            if (app == null && FirebaseApp.GetInstance("[DEFAULT]") == null)
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
            catch (Exception ex)
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
                var alarmList = result.DataRead.Where(x => x.State == (Ews.Common.EwsAlarmStateEnum.Active)).OrderByDescending(x => x.Transition);

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
            }
            catch (Exception ex)
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

        #region CreateEwsServer - Override
        protected override EwsServerDataAdapter CreateEwsServer()
        {
            return EwsServerDataAdapter.ConnectNew(ServerName, EwsAddress, "", UserName, Password,
                true, true, "Mongoose.Ews.Server.dll", "Mongoose.Ews.Server.MongooseEwsServiceHost");
        }
        #endregion

        #region EnsureServerParameters
        private void EnsureServerParameters()
        {
            CheckCancellationToken();
            DataAdapter.ModifyServerIsAutoStart(true);
            DataAdapter.ModifyServerAllowCookies(true);
            DataAdapter.ModifyServerPageSize(1000);
            DataAdapter.ModifyServerRootContainerItemAlternateId("RootContainer");
            DataAdapter.ModifyServerRootContainerItemDescription("All folders derive from here");
            EnsureSupportedMethods();
        }
        #endregion

        #region EnsureSupportedMethods
        /// <summary>
        /// Disable EWS Server functions that aren't needed in this solution and enable those that are.
        /// </summary>
        private void EnsureSupportedMethods()
        {
            CheckCancellationToken();

            DataAdapter.ModifyServerSupportedMethods(new EwsServerMethods
            {
                GetEnums = false,
                GetHierarchicalInformation = false,
                UnforceValues = false,

                ForceValues = true,
                AcknowledgeAlarmEvents = true,
                GetAlarmEventTypes = true,
                GetAlarmEvents = true,
                GetAlarmHistory = true,
                GetUpdatedAlarmEvents = true,
                GetContainerItems = true,
                GetHistory = true,
                GetItems = true,
                GetNotification = true,
                GetValues = true,
                GetWebServiceInformation = true,
                Renew = true,
                SetValues = true,
                Subscribe = true,
                Unsubscribe = true
            });
        }
        #endregion

    }
}
