using Mongoose.Common;
using Mongoose.Process;
using NUnit.Framework;
using SxL.Common;
using System;
using System.Threading;
using Mongoose.Test;
// using SmartConnector = Mongoose.Service.Mongoose;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process.Test
{
    /// <summary>
    /// This test fixture provides simple validation and execution tests for the EwsConsumeSample.  
    /// </summary>
    /// <remarks>
    /// NOTE:  This fixture requires access to the SmartConnector InMemoryCache.  In order to accomplish this, we need to 
    /// invoke the inversion of control initializer.  This requires a back reference to the Service project.  There is no NuGet package for this.   In order to test this
    /// you must perform the following:
    /// 
    /// 1.) Install Mongoose.Service (or it's binaries) on a test machine.  It can be the same development machine being used by this test fixture.
    /// 2.) Reference Mongoose.Service in this project (if not already done).  The version must be compatible with the NuGet packages referenced by this project.
    /// 3.) Uncomment the InitIoC call in the FixtureSetup.
    /// 4.) Remove Assertion statments stating "Please read the class code comments before running the tests in this fixture!"
    /// 5.) Update the app.config file and ensure that the database connection information is valid (if not already done).
    /// 6.) Confirm that the encryption key value matches that of the test database you are connecting to (see step 5) (if not already done).
    /// 
    /// At this point you should be able to run the fixture normally.
    /// </remarks>
    [TestFixture]
    public class EwsConsumeFixture : SmartConnectorTestFixtureBase
    {
        #region FixtureOneTimeSetup_Base - Override
        protected override void FixtureOneTimeSetup_Base()
        {
            MongooseObjectFactory.ConfigureDataDirectory();
            Assert.Fail("Please read the class code comments before running the tests in this fixture!");
            // SmartConnector.InitIoC();
        }
        #endregion

        #region ValidationTest
        /// <summary>
        /// Instantiates the processor and confirms that the defined validation we want is operational.
        /// </summary>
        [Test]
        public void ValidationTest()
        {
            try
            {
                var config = SampleConfigurations.EwsConsumeConfiguration();
                Assert.IsNotNull(config);

                // Some cursory validation
                var process = config.InstantiateInstance<EwsConsumeSampleProcessor>();
                Assert.IsNotNull(process);
                Assert.IsNotNull(process.DataFromEws);
                Assert.IsNotNull(process.DataFromEws.DataPointsToRead);
                Assert.AreEqual(3, process.DataFromEws.DataPointsToRead.Count);
                Assert.IsNotNull(process.DataFromEws.DataPointsToRead[0].Id);
                Assert.IsNotNull(process.DataFromEws.DataPointsToRead[0].Name);
                Assert.IsNotNull(process.DataToEws);
                Assert.AreEqual(1, process.DataToEws.DataPointsToWrite.Count);
                Assert.IsNotNull(process.DataToEws.DataPointsToWrite[0].Id);
                Assert.IsNotNull(process.DataToEws.DataPointsToWrite[0].Name);

                // Run the validator which does a deep validation.  There should be no issues at this point.
                var prompts = GenericValidator.ValidateItem(process);
                Assert.IsNotNull(prompts);
                Assert.AreEqual(0, prompts.Count);

                // Now we want to remove one of the Reader points.  This should cause a validation issue as the Processor declared it needs 3 of these.
                var inputPointRemoved = process.DataFromEws.DataPointsToRead[0];
                process.DataFromEws.DataPointsToRead.RemoveAt(0);

                // Re-validate
                prompts = GenericValidator.ValidateItem(process);
                Assert.IsNotNull(prompts);
                Assert.AreEqual(1, prompts.Count);
                var prompt = prompts[0];
                Assert.IsNotNull(prompt.Message);

                // Put the point back
                process.DataFromEws.DataPointsToRead.Add(inputPointRemoved);

                // Remove one from the output side
                var outputPointRemoved = process.DataToEws.DataPointsToWrite[0];
                process.DataToEws.DataPointsToWrite.RemoveAt(0);

                // Re-validate
                prompts = GenericValidator.ValidateItem(process);
                Assert.IsNotNull(prompts);
                Assert.AreEqual(1, prompts.Count);
                prompt = prompts[0];
                Assert.IsNotNull(prompt.Message);

                // Put it back
                process.DataToEws.DataPointsToWrite.Add(outputPointRemoved);

                // Re-validate
                prompts = GenericValidator.ValidateItem(process);
                Assert.IsNotNull(prompts);
                Assert.AreEqual(0, prompts.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogCategory.Testing, ex);
                throw;
            }
        }
        #endregion
        #region ExecutionTest
        /// <summary>
        /// Instantiates the Processor, and executes it.
        /// </summary>
        [Test]
        public void ExecutionTest()
        {
            try
            {
                var config = SampleConfigurations.EwsConsumeConfiguration();
                Assert.IsNotNull(config);

                var process = config.InstantiateInstance<EwsConsumeSampleProcessor>();
                Assert.IsNotNull(process);

                // Run it!
                var results = process.Execute(new CancellationTokenSource().Token, ProcessorExecutionMode.OnDemand);
                Assert.IsNotNull(results);
                Assert.IsTrue(results.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogCategory.Testing, ex);
                throw;
            }
        }
        #endregion
    }
}