using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Process.Ews;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Mongoose.Process;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    /// <summary>
    /// Sample Processor which illustrates how to consume an EWS endpoint.  Processing consists of reading three values from EWS, preforming a math 
    /// operation on two of them and writing the result back to EWS.
    /// </summary>
    [ConfigurationDefaults("EWS Consume Processor", "Example of how consume an EWS server with EwsClient library.")]
    public class EwsConsumeSampleProcessor : Processor
    {
        #region Constructor
        public EwsConsumeSampleProcessor()
        {
            DataFromEws = new ValueItemReader();
            DataToEws = new ValueItemWriter();
        }
        #endregion

        #region IsLicensed - Override
        public override bool IsLicensed => false;
        #endregion

        #region DataFromEws
        /// <summary>
        /// Data to be read from EWS.  This processer expects known inputs as defined by the RequiredDataPointList attribute.
        /// </summary>
        [Required, RequiredDataPointList("DataPointsToRead", "Input1", "Input2", "Function")]
        public ValueItemReader DataFromEws { get; set; }
        #endregion
        #region DataToEws
        /// <summary>
        /// Result data to be written to EWS.  This processer expects known outputs as defined by the RequiredDataPointList attribute.
        /// </summary>
        [Required, RequiredDataPointList("DataPointsToWrite", "Output1")]
        public ValueItemWriter DataToEws { get; set; }
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            var response = new List<Prompt>();

            // Fetch the data from the reader; in this case it's EWS data.
            var ewsData = DataFromEws.ReadData();

            string function = ewsData.DataRead.First(x => x.Name == "Function").GetValue();
            var a = ewsData.DataRead.First(x => x.Name == "Input1").GetValue();
            var b = ewsData.DataRead.First(x => x.Name == "Input2").GetValue();
            CheckCancellationToken();

            switch (function.ToUpper())
            {
                case "ADD":
                    DataToEws.DataPointsToWrite[0].Value = (a + b).ToString();
                    break;
                case "MULTIPLY":
                    DataToEws.DataPointsToWrite[0].Value = (a * b).ToString();
                    break;
                case "DIVIDE":
                    DataToEws.DataPointsToWrite[0].Value = (a / b).ToString();
                    break;
            }
            var writeResult = DataToEws.WriteData();
            response.AddRange(writeResult.Prompts);
            return response;
        }
        #endregion
    }
}
