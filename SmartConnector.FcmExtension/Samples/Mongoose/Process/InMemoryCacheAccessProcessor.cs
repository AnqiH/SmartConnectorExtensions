using System.Collections.Generic;
using System.Linq;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Process;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    /// <summary>
    /// Demonstrates how to access items from the InMemory Cache available to all Processor sub-classes
    /// </summary>
    [ConfigurationDefaults("InMemoryCache Access Processor", "Demonstrates how to access items from the InMemory Cache available to all Processor sub-classes.")]
    public class InMemoryCacheAccessProcessor : Processor
    {
        #region IsLicensed (Override)
        public override bool IsLicensed => false;
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {

            // Cache returns a cross thread accessible in memory cache.  Everyting, including reference types, can be serialized into the cache.  This means that an AddItem followed by a RetrieveItem will
            // never return the same instance of item.  This is intentional to prevent cross thread access issues.  All methods lock the internal storage for the duration of the call.

            // This means that your Processor and/or EWS Servers can exchange data by key wihtout having to write to the database.

            // You can retrieve items dynamically.  But if the item isn't in the cache NULL is returned.  That's a problem if your variable isn't NULLable.
            var person = Cache.RetrieveItem("Owner");

            // Alternatively, you can retrieve items generically.  Now, if the item isn't in the cache, the default for the type passed will be used.
            var owner = Cache.RetrieveItem<Person>("Owner");

            // Or you can pass in a Func so you control the default
            owner = Cache.RetrieveItem<Person>("Owner", () => new Person { Email = "foo@na.com" });
            
            // Or a delegate to a method
            owner = Cache.RetrieveItem<Person>("Owner", DefaultPerson);
            
            
            // Adding and updating items is just as easy
            Cache.AddOrUpdateItem(person, "Owner");

            // You can also run LINQ against the Keys which is IQueryable
            var items = new List<Person>();
            foreach (var key in Cache.Keys().Where(x => x.StartsWith("Boo")))
            {
                items.Add(Cache.RetrieveItem(key));
            }

            // And of course delete an item.
            Cache.DeleteItem("Owner");

            return new List<Prompt>();
        }
        #endregion

        #region DefaultPerson
        private Person DefaultPerson()
        {
            return new Person
            {
                Email = "EmailRequired@na.com",
                FirstName = "John",
                LastName = "Doe"
            };
        }
        #endregion
    }
}
