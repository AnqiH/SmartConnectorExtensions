using System;
using System.Collections.Generic;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Process;
using SxL.Common;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    /// <summary>
    /// Demonstrates how to consume the build in logging framework. 
    /// </summary>
    [ConfigurationDefaults("Logging Processor", "Demonstrates how to consume the build in logging framework.")]
    public class LoggingProcessor : Processor
    {
        #region IsLicensed (Override)
        public override bool IsLicensed => false;
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            try
            {
                // Multiple levels of logging.  Framework figures out what to actually write.  The more verbose is based on the value of LoggingLevel enum
                Logger.LogInfo(LogCategory.Processor, $"{Name} started");
                Logger.LogStatus(LogCategory.Processor, $"{Name} started");
                Logger.LogTrace(LogCategory.Processor, $"{Name} started");
                Logger.LogDebug(LogCategory.Processor, $"{Name} started");

                // Anything else passed to any log method gets JSON serialized and written to the log
                Logger.LogInfo(LogCategory.Reader, "Passed attributes", new Person(), DateTime.UtcNow);

                // If you need to know what's configured you can
                var maxLevelConfigured = Logger.MaximumLoggingLevel;

                // Or even where the files are written to
                var whereAreTheLogFiles = Logger.LogDestinationFolder;

                // Categories can also be custom. 
                Logger.LogInfo("MyCustomCategory", "Hello World");
            }
            catch (Exception ex)
            {
                // Log Exceptions you want to handle
                Logger.LogError(LogCategory.Processor, ex);
                throw;
            }
            return new List<Prompt>();
        }
        #endregion
    }

}
