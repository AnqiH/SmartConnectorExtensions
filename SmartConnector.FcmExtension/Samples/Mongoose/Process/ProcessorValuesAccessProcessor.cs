using Mongoose.Common;
using Mongoose.Process;
using SxL.Common;
using System;
using System.Collections.Generic;
using Mongoose.Common.Attributes;
using Mongoose.Common.Data;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    /// <summary>
    /// Demonstrates how to add a value to the ProcessorValues store when state needs to be maintained.
    /// </summary>
    [ConfigurationDefaults("ProcessorValuesAccess Processor", "Example of how to lazily retrieve/create ProcessorValues from a Processor")]
    public class ProcessorValuesAccessProcessor : Processor
    {
        #region IsLicensed - Override
        public override bool IsLicensed => false;
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            const string myKey = "MyKey";
            
            var pv = this.FindOrCreateProcessorValue(myKey);
            pv.Value = Guid.NewGuid().ToNormalizedGuid();
            ProcessorValueSource.Save();

            return new List<Prompt>
            {
                new Prompt
                {
                    Message = $"Value of {pv.Value} was assigned to ProcessorValue.ID = {pv.Id}",
                    Severity = PromptSeverity.MayContinue
                }
            };
        }
        #endregion
    }
}
