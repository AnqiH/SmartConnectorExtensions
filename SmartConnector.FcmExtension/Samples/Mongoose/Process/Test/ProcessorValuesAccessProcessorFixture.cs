using Mongoose.Common;
using Mongoose.Configuration;
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
    /// This test fixture provides simple validation and execution tests for the ProcessorValuesAccessProcessor library.  
    /// </summary>
    /// <remarks>
    /// NOTE:  This fixture requires access to the SmartConnector persistant store.  In order to accomplish this, we need to 
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
    public class ProcessorValuesAccessProcessorFixture : SmartConnectorTestFixtureBase
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
            var config = ProcessConfiguration.ExtractConfiguration<ProcessorValuesAccessProcessor>();
            Assert.IsNotNull(config);

            // Some cursory validation
            var process = config.InstantiateInstance<ProcessorValuesAccessProcessor>();
            Assert.IsNotNull(process);
            var results = GenericValidator.ValidateItem(process);
            Assert.AreEqual(0, results.Count);
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
                var process = new ProcessorValuesAccessProcessor();
                Assert.IsNotNull(process);

                // Run it!
                var results = process.Execute(new CancellationTokenSource().Token, ProcessorExecutionMode.OnDemand);
                Assert.IsNotNull(results);
                Assert.IsTrue(results.Success);
                Assert.AreEqual(1, results.Prompts.Count);
                Logger.LogDebug(LogCategory.Testing, results.Prompts[0].Message);
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
