using System.Collections.Generic;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Process;

namespace Mongoose.Process.Sample
{
    /// <summary>
    /// Demonstrates the use of ActionBroker to start/stop other Processors or EwsServers
    /// </summary>
    [ConfigurationDefaults("ActionBroker Sample Processor", "Demonstrates the use of ActionBroker to start a configuration from a running Proccessor.")]
    public class ActionBrokerSampleProcessor : Processor
    {
        #region IsLicensed (Override)
        public override bool IsLicensed => false;
        #endregion

        #region Username
        [EncryptedString]
        public string Username { get; set; }
        #endregion
        #region Password
        [EncryptedString]
        public string Password { get; set; }
        #endregion

        #region ConfigurationsToSpawn
        [CollectionLength(1)]
        public List<int> ConfigurationsToSpawn { get; set; }
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            foreach (var configId in ConfigurationsToSpawn)
            {
                // If it's already running
                if (ActionBroker.IsConfigurationRunning(configId))
                {
                    // We'll stop it and wait for it to stop completely
                    ActionBroker.StopConfiguration(configId, DerivedFromConfigurationType.Processor);
                    do
                    {
                        NoBusyWait(200);
                    } while (ActionBroker.IsConfigurationRunning(configId));
                }

                // But then we'll start it up again.

                // Credentials are optional.  They're used for logging.  If you want to log who spwaned what you can use them
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    ActionBroker.StartConfiguration(configId, DerivedFromConfigurationType.Processor, Username, Password);
                }
                else
                {
                    ActionBroker.StartConfiguration(configId, DerivedFromConfigurationType.Processor);
                }
            }

            // All of what we show for Processors is also available for EwsServers.

            return new List<Prompt>();
        }
        #endregion
    }
}
