using Mongoose.Configuration;
using Mongoose.Test.Processors;
using NUnit.Framework;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    public class SampleConfigurations
    {
        private const string EwsUrlEndpoint = "http://localhost:8081/EcoStruxure/DataExchange";
        private const string EwsUsername = "TODO";
        private const string EwsPassword = "TODO";
        
        #region EwsConsumeConfiguration
        /// <summary>
        /// Provides a full ProcessConfiguration instance for the EwsConsumeSample class.
        /// </summary>
        public static ProcessConfiguration EwsConsumeConfiguration()
        {
            var config = ProcessConfiguration.ExtractConfiguration<EwsConsumeSampleProcessor>();
            config.Name = "EwsConsumeSampleProcessor";
            config.Description = "Sample processor that adds, subtracts or multiplies two variables (sourced from EWS) and writes the result back to EWS";

            // Hydrate the properties (and verify they were properly added)
            var dataItemReader = config.EnsureParameterSet("DataFromEws");
            dataItemReader.EnsureParameter<string>("Address", false, true).Value = EwsUrlEndpoint;
            dataItemReader.EnsureParameter<string>("UserName", true, true).Value = EwsUsername;
            dataItemReader.EnsureParameter<string>("Password", true, true).Value = EwsPassword;
            var pointsToRead = dataItemReader.EnsureParameterSet("DataPointsToRead", true, 3);
            Assert.AreEqual(3, pointsToRead.ParameterSets.Count);

            pointsToRead.ParameterSets[0].EnsureParameter<string>("Name").Value = "Input1";
            pointsToRead.ParameterSets[0].EnsureParameter<string>("Id").Value = "01/Server 1/SmartConnector/EwsConsumeSample/Input1";

            pointsToRead.ParameterSets[1].EnsureParameter<string>("Name").Value = "Input2";
            pointsToRead.ParameterSets[1].EnsureParameter<string>("Id").Value = "01/Server 1/SmartConnector/EwsConsumeSample/Input2";

            pointsToRead.ParameterSets[2].EnsureParameter<string>("Name").Value = "Function";
            pointsToRead.ParameterSets[2].EnsureParameter<string>("Id").Value = "01/Server 1/SmartConnector/EwsConsumeSample/Function";

            // Hydrate the properties (and verify they were properly added)
            var dataItemWriter = config.EnsureParameterSet("DataToEws");
            dataItemWriter.EnsureParameter<string>("Address", false, true).Value = EwsUrlEndpoint;
            dataItemWriter.EnsureParameter<string>("UserName", true, true).Value = EwsUsername;
            dataItemWriter.EnsureParameter<string>("Password", true, true).Value = EwsPassword;
            var pointsToWrite = dataItemWriter.EnsureParameterSet("DataPointsToWrite", true, 1);
            Assert.AreEqual(1, pointsToWrite.ParameterSets.Count);

            pointsToWrite.ParameterSets[0].EnsureParameter<string>("Name").Value = "Output1";
            pointsToWrite.ParameterSets[0].EnsureParameter<string>("Id").Value = "01/Server 1/SmartConnector/EwsConsumeSample/Output1";

            return config;
        }
        #endregion
    }
}
