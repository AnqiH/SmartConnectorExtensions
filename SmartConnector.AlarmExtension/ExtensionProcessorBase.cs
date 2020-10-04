using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ews.Common;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Ews.Server.Data;
using Mongoose.Ews.Server.Data.Shared;
using Mongoose.Process;
using SxL.Common;
//using FirebaseAdmin;
//using FirebaseAdmin.Messaging;
//using Google.Apis.Auth.OAuth2;

namespace SmartConnector.AlarmExtension
{
    public abstract class ExtensionProcessorBase : Processor
    {
        #region IsLicensed - Override
        public override bool IsLicensed => false;
        #endregion

        #region Server name
        [Required, DefaultValue("SmartConnector FCM Service"), Tooltip("Name of the EWS Server to connect to or bootstrap")]
        public string ServerName { get; set; }
        #endregion

        #region UserName
        [Required, EncryptedString, DefaultValue("admin")]
        public string UserName { get; set; }
        #endregion

        #region Password
        [Required, EncryptedString, DefaultValue("admin123")]
        public string Password { get; set; }
        #endregion

        #region Validate - Override
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && UserName == Password)
            {
                yield return new ValidationResult("UserName and Password cannot be the same", new[] { "UserName", "Password" });
            }

            foreach (var validationResult in base.Validate(validationContext))
            {
                yield return validationResult;
            }
        }
        #endregion

        #region DataAdapter
        private EwsServerDataAdapter _dataAdapter = null;
        /// <summary>
        /// Lazyily created instance of an EwsServerDataAdapter
        /// instructed the EwsServerDataAdapter to create an instance of a default native SmartConnector EWS Server
        /// </summary>
        protected EwsServerDataAdapter DataAdapter
        {
            get
            {
                if (_dataAdapter != null) return _dataAdapter;
                try
                {
                    _dataAdapter = EwsServerDataAdapter.ConnectExisting(ServerName, UserName, Password);
                }
                catch (ApplicationException ex)
                {
                    // We'll log the error, but continue on
                    Logger.LogError(LogCategory.Processor, ex);
                }
                return _dataAdapter ?? (_dataAdapter = CreateEwsServer());
            }
            set { _dataAdapter = value; }
        }
        #endregion

        #region IsConnected
        /// <summary>
        /// Returns true if DataAdapter has connected to an EWS Server.  
        /// </summary>
        protected bool IsConnected => DataAdapter != null;
        #endregion


        #region CreateEwsServer - Virtual
        protected virtual EwsServerDataAdapter CreateEwsServer()
        {
            return null;
        }
        #endregion

        #region CreateCannotConnectPrompt
        protected Prompt CreateCannotConnectPrompt()
        {
            return new Prompt
            {
                Message = "Failed to connect to a valid EwsServerDataAdapter.",
                Severity = PromptSeverity.MayNotContinue
            };
        }
        #endregion

        #region Dispose - Override
        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed) return;
            _dataAdapter?.Dispose();
            _disposed = true;
        }
        #endregion
    }
}
