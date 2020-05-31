using Ews.Common;
using Mongoose.Ews.Server.Data;
using NUnit.Framework;
using SxL.Common;
using System;
using System.Linq;
using Mongoose.Common;
using Mongoose.Test;
// using SmartConnector = Mongoose.Service.Mongoose;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process.Test
{
    /// <summary>
    /// This test fixture provides some examples of how to consume the EwsServerAdapter in order to create, access and manipulate your EWS Server in a SmartConnector Processor.
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
    public class EwsServerAdapterFixture : SmartConnectorTestFixtureBase
    {
        #region FixtureOneTimeSetup_Base - Override
        protected override void FixtureOneTimeSetup_Base()
        {
            MongooseObjectFactory.ConfigureDataDirectory();
            Assert.Fail("Please read the class code comments before running the tests in this fixture!");
            // SmartConnector.InitIoC();
        }
        #endregion

        #region CreateNewEwsServerTest
        /// <summary>
        /// Test to demonstrate how to create a new EWS Server instance and manipulate some of it's properties.
        /// </summary>
        [Test]
        public void CreateNewEwsServerTest()
        {
            // You can bootstrap a new EWS server or connect to an existing one.
            using (var adapter = EwsServerDataAdapter.ConnectNew(Guid.NewGuid().ToNormalizedGuid(), "http://localhost://50999/MyRoute", "MyRealm", "admin", "Admin!23", true))
            {
                // After I've conected I can modify anyof the "setup" stuff...
                adapter.ModifyServerName("My EWS Server");
                adapter.ModifyServerRealm("MyRealm");
                adapter.ModifyServerAddress("http:/localhost:15999/MyCustomRoute");
                // The namespace is not modifyable  by the adapter
                Assert.That(adapter.UsedNameSpace, Is.Not.Null.Or.Empty);


                // I can add more users as well
                var newuser = adapter.AddUser(Guid.NewGuid().ToNormalizedGuid(), "HeLL0World");

                // The "root" ContainerItem is also configurable
                var myServer = adapter.ModifyServerRootContainerItemDescription("New Description Value");
                myServer = adapter.ModifyServerRootContainerItemAlternateId("Root");
                // NOTE: "AlternateId" is what we server via EWS as the "id" of any object.  This allows you to come up with your own ID formats.

                // I can even "Purge" the server of all data
                adapter.PurgeData(); // Doesn't purge users
                adapter.PurgeAll(); // Doesn't purge the current user

                // I can even "Delete" the Server entirely
                adapter.DeleteServer();

                Assert.IsFalse(adapter.IsConnected);
            }
        }
        #endregion

        #region ConnectEwsServerTest
        /// <summary>
        /// Test to demonstrate how to connect to an existing EWS Server.
        /// </summary>
        [Test]
        public void ConnectEwsServerTest()
        {
            // You can connect to an existing EWS server as long as you know it's name and credentials to authenticate with
            using (var adapter = ConnectToDefaultServer())
            {
                // You would do your work in here.
            }
            // Disposing of the adapter will effectively log you out.
        }
        #endregion

        #region PasswordStrengthTest
        /// <summary>
        /// Test to demonstrate how to confirm password candidates meet validation rules.
        /// </summary>
        [Test]
        public void PasswordStrengthTest()
        {
            // You can confirm that a password candidate is valid by calling into the adapter.

            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword(string.Empty));

            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("ABCDEFGHI"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("abcdefghi"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("12345678"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("!@#$%^&*"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("ABCDefgh"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("abcdEFGH"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("ABCD5678"));
            Assert.IsFalse(EwsServerDataAdapter.IsValidPassword("abcd5678"));
            Assert.IsTrue(EwsServerDataAdapter.IsValidPassword("ABcd5678"));
            Assert.IsTrue(EwsServerDataAdapter.IsValidPassword("AbCd45&*"));
        }
        #endregion

        #region ContainerItemTest
        /// <summary>
        /// Test to demonstrate how to create and manipulate a ContainerItem.
        /// </summary>
        [Test]
        public void ContainerItemTest()
        {
            using (var adapter = ConnectToDefaultServer())
            {
                var altId = Guid.NewGuid().ToString();
                var parent = adapter.AddContainerItem(altId, "Name", "Description", EwsContainerTypeEnum.Folder);
                Assert.IsNotNull(parent);
                Assert.AreEqual(parent.AlternateId, altId);
                Assert.AreEqual(parent.Name, "Name");
                Assert.AreEqual(parent.Description, "Description");
                Assert.AreEqual(parent.Type, EwsContainerTypeEnum.Folder);
                Assert.IsNull(parent.ContainerItems);

                // Modify the Description
                parent = adapter.ModifyContainerItemDescription(parent, "New Description");
                Assert.AreEqual("New Description", parent.Description);

                // Modify the Name
                var newName = Guid.NewGuid().ToString();
                parent = adapter.ModifyContainerItemName(parent, newName);
                Assert.AreEqual(newName, parent.Name);

                // Modify the Type
                parent = adapter.ModifyContainerItemType(parent, EwsContainerTypeEnum.Device);
                Assert.AreEqual(EwsContainerTypeEnum.Device, parent.Type);

                // Modify the Parent
                var child = adapter.AddContainerItem(Guid.NewGuid().ToString(), "Name", "Description", EwsContainerTypeEnum.Server, parent);
                Assert.IsNotNull(child);
                Assert.AreEqual(child.Type, EwsContainerTypeEnum.Server);
                Assert.IsNull(child.ContainerItems);
                Assert.IsNotNull(child.Parent);

                Assert.IsNotNull(parent.ContainerItems);
                Assert.AreEqual(1, parent.ContainerItems.Count);
                Assert.AreEqual(child.Id, parent.ContainerItems[0].Id);
            }
        }
        #endregion
        #region ValueItemTest
        /// <summary>
        /// Test to demonstrate how to create and manipulate a ValueItem.
        /// </summary>
        [Test]
        public void ValueItemTest()
        {
            using (var adapter = ConnectToDefaultServer())
            {

                var altId = Guid.NewGuid().ToString();
                var valueItem = adapter.AddValueItem(altId, "Name", "Description", EwsValueTypeEnum.String, EwsValueWriteableEnum.Writeable, EwsValueForceableEnum.Forceable, EwsValueStateEnum.Good, null, null);
                Assert.IsNotNull(valueItem);
                Assert.IsNull(valueItem.Parent);

                // Modify the Description
                valueItem = adapter.ModifyValueItemDescription(valueItem, "New Description");
                Assert.AreEqual("New Description", valueItem.Description);

                // Modify the Name (NULL)
                try
                {
                    valueItem = adapter.ModifyValueItemName(valueItem, "");
                    Assert.Fail();
                }
                catch (Exception)
                {
                    // Expected
                }

                // Modify the Name
                var newName = Guid.NewGuid().ToString();
                valueItem = adapter.ModifyValueItemName(valueItem, newName);
                Assert.AreEqual(newName, valueItem.Name);

                // Moidfy the Value
                valueItem = adapter.ModifyValueItemValue(valueItem, "Somevalue");
                Assert.AreEqual("Somevalue", valueItem.Value);

                // Modify the State
                valueItem = adapter.ModifyValueItemState(valueItem, EwsValueStateEnum.Offline);
                Assert.AreEqual(EwsValueStateEnum.Offline, valueItem.State);

                // Modify the Value and State at the same time (Results in one HistoryRecord)
                valueItem = adapter.ModifyValueItemValue(valueItem, "Different Value", EwsValueStateEnum.Uncertain);

                // Modify the Value (or any property) by AlternateId
                var alternateId = valueItem.AlternateId;
                valueItem = adapter.ModifyValueItemValue(alternateId, "Changed again");

                // Modify the Writeable
                valueItem = adapter.ModifyValueItemWriteable(valueItem, EwsValueWriteableEnum.ReadOnly);
                Assert.AreEqual(EwsValueWriteableEnum.ReadOnly, valueItem.Writeable);

                // Modify the Type
                valueItem = adapter.ModifyValueItemType(valueItem, EwsValueTypeEnum.Integer);
                Assert.AreEqual(EwsValueTypeEnum.Integer, valueItem.Type);

                try
                {
                    adapter.ModifyValueItemValue(valueItem, "Somevalue again");
                    Assert.Fail();
                }
                catch (Exception)
                {
                }

                // Modify the Parent
                var parent = adapter.ContainerItems.Any()
                    ? adapter.ContainerItems.FirstOrDefault()
                    : adapter.AddContainerItem(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "Description",
                        EwsContainerTypeEnum.Folder);
                var preAddCount = parent.ValueItems == null ? 0 : parent.ValueItems.Count();

                valueItem = adapter.ModifyValueItemParent(valueItem, parent);
                Assert.IsNotNull(valueItem.Parent);
                Assert.AreEqual(valueItem.Parent.Id, parent.Id);
            }
        }
        #endregion
        #region HistoryItemTest
        /// <summary>
        /// Test to demonstrate how to create and manipulate a ValueItem.
        /// </summary>
        [Test]
        public void HistoryItemTest()
        {
            using (var adapter = ConnectToDefaultServer())
            {
                var viAltId = Guid.NewGuid().ToString(); var valueItem = adapter.AddValueItem(viAltId, "Name", "Description", EwsValueTypeEnum.Integer, EwsValueWriteableEnum.Writeable, EwsValueForceableEnum.Forceable, EwsValueStateEnum.Good, null, null);
                Assert.IsNotNull(valueItem);
                Assert.IsNull(valueItem.Parent);

                var hiAltId = Guid.NewGuid().ToString();
                var historyItem = adapter.AddHistoryItem(hiAltId, null, null, valueItem);
                Assert.IsNotNull(historyItem);
                Assert.IsNotNull(historyItem.ValueItem);
                Assert.AreEqual(valueItem.Id, historyItem.ValueItem.Id);

                // Modify the Description
                historyItem = adapter.ModifyHistoryItemDescription(historyItem, "New Description");
                Assert.AreEqual("New Description", historyItem.Description);

                // Modify the Name
                var newName = Guid.NewGuid().ToString();
                historyItem = adapter.ModifyHistoryItemName(historyItem, newName);
                Assert.AreEqual(newName, historyItem.Name);

                valueItem = adapter.ModifyValueItemValue(valueItem, 1);
                valueItem = adapter.ModifyValueItemValue(valueItem, 2);
                valueItem = adapter.ModifyValueItemValue(valueItem, 3);
                valueItem = adapter.ModifyValueItemValue(valueItem, 4);

                historyItem = adapter.HistoryItems.FirstOrDefault(x => x.Id == historyItem.Id);
                Assert.AreEqual(4, adapter.HistoryRecords.Count(x => x.EwsHistoryItem.Id == historyItem.Id));

                historyItem = adapter.HistoryItems.FirstOrDefault(x => x.Id == historyItem.Id);
                Assert.IsNotNull(historyItem);
                Assert.IsNotNull(historyItem.ValueItem);
                Assert.AreEqual(viAltId, historyItem.ValueItem.AlternateId);

                adapter.DeleteHistoryItem(historyItem);

                valueItem = adapter.ValueItems.FirstOrDefault(x => x.Id == valueItem.Id);
                Assert.IsNotNull(valueItem);
            }
        }
        #endregion

        #region ConnectToDefaultServer
        /// <summary>
        /// Returns an EwsServerDataAdpater connected to the default EWS Server that is installed with SmartConnector.  
        /// NOTE: If you change the name or credentials, then please update this code.
        /// </summary>
        private EwsServerDataAdapter ConnectToDefaultServer()
        {
            var adapater = EwsServerDataAdapter.ConnectExisting("SmartConnector EWS Test Server", "admin", "Admin!23");
            Assert.IsTrue(adapater.IsConnected);
            return adapater;
        }
        #endregion
    }
}
