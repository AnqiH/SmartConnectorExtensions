using System;
using Ews.Client;
using Ews.Common;
using NUnit.Framework;
using System.Linq;
using Mongoose.Common;
using Mongoose.Test;
using SxL.Common;
// using SmartConnector = Mongoose.Service.Mongoose;


namespace SmartConnector.FcmExtension.Samples.Mongoose.Process.Test
{
    /// <summary>
    /// This test fixture shows examples of how to make EWS calls with an IManagedEwsClient instance.  This library provides finer grained control over EWS calls while still abstracting away 
    /// the complexities.  You should also consider using the ValueItemReader, ValueItemWriter, HistoryItemReader and others for your Processor development.
    /// </summary>
    /// <remarks>
    /// Ews.Client can be obtained as it's own NuGet or as dependency to Mongoose.Process.
    /// </remarks>
    [TestFixture]
    public class EwsClientFixture : SmartConnectorTestFixtureBase, IEndpoint
    {
        private IManagedEwsClient _ewsClient;

        #region Address (IEndpoint Member)
        private string _address;
        /// <inheritdoc />
        string IEndpoint.Address
        {
            get => _address;
            set => _address = value;
        }
        #endregion
        #region UserName (IEndpoint Member)
        private string _userName;

        /// <inheritdoc />
        string IEndpoint.UserName
        {
            get => _userName;
            set => _userName = value;
        }
        #endregion
        #region Password (IEndpoint Member)
        private string _password;

        /// <inheritdoc />
        string IEndpoint.Password
        {
            get => _password;
            set => _password = value;
        }
        #endregion

        #region FixtureOneTimeSetup_Base - Override
        protected override void FixtureOneTimeSetup_Base()
        {
            Assert.Fail("Please read the class code comments before running the tests in this fixture!");
            // SmartConnector.InitIoC();
            _ewsClient = MongooseObjectFactory.Current.GetInstance<IManagedEwsClient>();

            _userName = "admin";
            _password = "Admin!23";
            _address = "http://localhost:8081/EcoStruxure/DataExchange";
        }
        #endregion

        #region GetEwsServerVersion
        [Test]
        public void GetEwsServerVersion()
        {

            // By default, the managed client will be the max version supported by the server
            Assert.AreEqual(EwsVersion.Ews12, _ewsClient.EwsVersionImplemented(this));

            // This means that SupportedProfiles (which is an EWS 1.2 concept) will return data
            Assert.IsNotNull(_ewsClient.SupportedProfiles(this));

            // But we can downgrade each call manually
            Assert.IsNull(_ewsClient.SupportedProfiles(this, EwsVersion.Ews11));
        }
        #endregion
        #region GetWebServiceInformationTest
        [Test]
        public void GetWebServiceInformationTest()
        {
            var response = _ewsClient.GetWebServiceInformation(this);
            Assert.IsNotNull(response);

            response = _ewsClient.GetWebServiceInformation(this);

            Assert.IsNotNull(response.GetWebServiceInformationVersion);
            Assert.IsNotNull(response.GetWebServiceInformationVersion.MajorVersion);
            Assert.IsNotNull(response.GetWebServiceInformationVersion.MinorVersion);
            Assert.IsNotNull(response.GetWebServiceInformationVersion.UsedNameSpace);

            Assert.IsNotNull(response.GetWebServiceInformationSupportedOperations);

            // Regardless of what EWS Server Endpoint points to, there are always a minimum number of Supported Operations
            Assert.GreaterOrEqual(response.GetWebServiceInformationSupportedOperations.Length, 4);

            // According to the EWS specification, GetWebServiceInformation should be returned but as of v1.6.1 SBO doesn't comply
            //Assert.IsTrue(response.GetWebServiceInformationSupportedOperations.Contains("GetWebServiceInformation"));
            Assert.IsTrue(response.GetWebServiceInformationSupportedOperations.Contains("GetContainerItems"));
            Assert.IsTrue(response.GetWebServiceInformationSupportedOperations.Contains("GetItems"));
            Assert.IsTrue(response.GetWebServiceInformationSupportedOperations.Contains("GetValues"));

            // The connection caches this information for you so you won't need to make multiple calls etc.
            var supportedMethods = _ewsClient.SupportedMethods(this).ToList();
            Assert.IsTrue(supportedMethods.Contains("GetContainerItems"));
            Assert.IsTrue(supportedMethods.Contains("GetItems"));
            Assert.IsTrue(supportedMethods.Contains("GetValues"));

            // You don't need to actually call GetWebServiceInformation explicitly, the Connection manages that for you when you access the cached values it returns.
            var version = _ewsClient.ServerVersion(this);
            Assert.GreaterOrEqual(version.Major, 1);
            Assert.GreaterOrEqual(version.Minor, 1);
        }
        #endregion
        #region GetContainerItems_RootTest
        [Test]
        public void GetContainerItems_RootTest()
        {
            var response = _ewsClient.GetContainerItems(this);
            Assert.IsNotNull(response);

            // No errors
            Assert.IsNotNull(response.GetContainerItemsErrorResults);
            Assert.AreEqual(0, response.GetContainerItemsErrorResults.Length);

            // 1 container item
            Assert.IsNotNull(response.GetContainerItemsItems);
            Assert.AreEqual(1, response.GetContainerItemsItems.Length);

            var ci = response.GetContainerItemsItems[0];
            Assert.IsNotNull(ci.Id);
            Assert.AreEqual(EwsContainerTypeEnum.Server.ToEwsString(), ci.Type);

            Assert.IsNotNull(ci.Items);
            Assert.IsNotNull(ci.Items.AlarmItems);
            Assert.IsNotNull(ci.Items.ContainerItems);
            Assert.IsNotNull(ci.Items.HistoryItems);
            Assert.IsNotNull(ci.Items.ValueItems);
        }
        #endregion

        #region GetContainerItems_DiscoverChildrenTest
        [Test]
        public void GetContainerItems_DiscoverChildrenTest()
        {
            // Get the root ContainerItem
            var getCIResponse = _ewsClient.GetContainerItems(this);
            Assert.IsNotNull(getCIResponse);

            Assert.IsNotNull(getCIResponse.GetContainerItemsItems);
            Assert.AreEqual(1, getCIResponse.GetContainerItemsItems.Length);

            var rootCI = getCIResponse.GetContainerItemsItems[0];
            Assert.IsNotNull(rootCI.Items.ContainerItems);
            if (rootCI.Items.ContainerItems.Length > 0)
            {
                getCIResponse = _ewsClient.GetContainerItems(this, rootCI.Items.ContainerItems.Select(x => x.Id).ToArray());

                // We could also get the ValueItems at the root as well.
                var getItemsResponse = _ewsClient.GetItems(this, rootCI.Items.ValueItems.Select(x => x.Id).ToArray());
            }
        }
        #endregion
    }
}
