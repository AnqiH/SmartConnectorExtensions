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
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using SxL.Common;
using Mongoose.Process.Ews;
using Mongoose.Common.Api;
using System.Threading;

namespace SmartConnector.FcmExtension
{
    public class UpdateProcessor : ExtensionProcessorBase, ILongRunningProcess
    {
        #region EwsAddress
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("http://localhost:81/EcoStruxure/DataExchange"), Tooltip("Default HTTP address to configure when bootstraping the EWS Server")]
        public string EwsAddress { get; set; }
        #endregion

        private List<Prompt> prompts = new List<Prompt>();

        private ICache _cache;


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

            _cache = MongooseObjectFactory.Current.GetInstance<ICache>();

            /*
            try
            {
                messaging = FirebaseMessaging.GetMessaging(app);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogCategory.Processor, this.Name, $"Failed to connect to FCM service.");
                prompts.Add(ex.ToPrompt());
                return prompts;
            }
            */

            try
            {
                //readAlarms(alarmReader);

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
            Logger.LogStatus(LogCategory.Processor, "update processor", $"result is {result}");

            var lastUpdated = alarmReader.LastUpdate;
            Logger.LogStatus(LogCategory.Processor, "update processor", $"last update {lastUpdated}");


            if (result.Success)
            {
                var alarmList = result.DataRead.Where(x => x.State==(Ews.Common.EwsAlarmStateEnum.Active)).OrderByDescending(x => x.Transition);
    
                Logger.LogStatus(LogCategory.Processor, "update processor", $"active alarms {alarmList.Count()}");

                if (alarmList.Count() > 0)
                {
                    var lastAlarm = alarmList.ElementAt(0);
                    Logger.LogStatus(LogCategory.Processor, "update processor", $"last alarm {lastAlarm.Message}");
                }

                //_cache.AddOrUpdateItem(lastAlarm, "key", true);
                //Logger.LogStatus(LogCategory.Processor, "update process", $"cache item {_cache.RetrieveItem("key").Message}");


  /*
                if (!SendNotification(message, Messaging).Result)
                {
                    prompts.Add(new Prompt { Message = "message sent unsuccessful" });
                }

    */
            }
            else
            {
                Logger.LogDebug(LogCategory.Processor, $"Alarm read failed.");
            }
        }

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
        

        protected Message createMessage(string title, string body)
        {
            // create notification message
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body =body
                },
                Topic = "test_topic"
            };

            return message;
        }


    }
}
