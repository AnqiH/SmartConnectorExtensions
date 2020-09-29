using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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

namespace SmartConnector.FcmExtension
{
    [ConfigurationDefaults("FCM Setup Processor", "Ensures the presence of an EWS Server and certain contents, bootstrapping whatever needs to be there.")]
    public class SetupProcessor : ExtensionProcessorBase
    {

        #region EwsAddress
        /// <summary>
        /// The address 
        /// </summary>
        [Required, DefaultValue("http://localhost:1900/SmartConnectorFcmService"), Tooltip("Default HTTP address to configure when bootstraping the EWS Server")]
        public string EwsAddress { get; set; }
        #endregion


        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            // Make sure to connect to a EWS server
            if (!IsConnected) return new List<Prompt> { CreateCannotConnectPrompt() };
            //throw new NotImplementedException();
            if (!DataAdapter.Server.IsRunning) DataAdapter.StartServer();

            EnsureServerParameters();
 
            return new List<Prompt>();
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
