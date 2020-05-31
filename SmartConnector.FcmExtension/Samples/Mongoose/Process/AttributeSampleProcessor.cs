using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mongoose.Common;
using Mongoose.Common.Attributes;
using Mongoose.Process;

namespace SmartConnector.FcmExtension.Samples.Mongoose.Process
{
    /// <summary>
    /// Demonstrates the use of several annotation attributes which control configuration and appearance of configurations in the portal.
    /// </summary>
    /// <remarks>
    /// All public read/write properties are extracted automatically into a configuration.  
    /// 
    /// Attributes can be used to control validation: Required, MaxLength, Range (most anything from System.ComponentModel including any of your own sub-classes).
    /// 
    /// You can also exclude something that might have normally been extracted using ConfigurationIgnore
    /// 
    /// Attributes can also be used to seed configuration data like DefaultValue and ProcessConfigurationDefaults.
    /// Encrypt configuration data with EncryptedString
    /// Or even provide context to the Portal user with Tooltip
    /// </remarks>
    [ConfigurationDefaults("Attribute Sample Processor", "Demonstrates the use of several annotation attributes which control configuration and appearance of configurations in the portal.")]
    public class AttributeSampleProcessor : Processor
    {
        #region IsLicensed (Override)
        public override bool IsLicensed => false;
        #endregion

        #region IntegerPropertyWithRange
        [Range(0, 100)]
        public int IntegerPropertyWithRange { get; set; }
        #endregion
        #region IntegerPropertyWithoutRange
        public int IntegerPropertyWithoutRange { get; set; }
        #endregion
        #region NullableIntegerPropertyWithRange
        [Range(0, 100), DefaultValue(10)]
        public int? NullableIntegerPropertyWithRange { get; set; }
        #endregion
        #region NullableIntegerPropertyWithoutRange
        public int? NullableIntegerPropertyWithoutRange { get; set; }
        #endregion

        #region NotRequiredStringWithMinLength
        [MinLength(15)]
        public string NotRequiredStringWithMinLength { get; set; }
        #endregion
        #region NotRequiredStringWithMaxLength
        [MaxLength(15)]
        public string NotRequiredStringWithMaxLength { get; set; }
        #endregion
        #region RequiredStringWithMinLength
        [Required, MinLength(15)]
        public string RequiredStringWithMinLength { get; set; }
        #endregion
        #region RequiredString
        [Required]
        public string RequiredString { get; set; }
        #endregion
        #region RequiredStringWithMaxLength
        [Required, MaxLength(15)]
        public string RequiredStringWithMaxLength { get; set; }
        #endregion
        #region MasterCredential
        [Required, Tooltip("Master credentials needed")]
        public Credential MasterCredential { get; set; }
        #endregion

        #region Execute_Subclass - Override
        protected override IEnumerable<Prompt> Execute_Subclass()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class Credential
    {
        #region Name
        [Tooltip("Name of the user")]
        public string Name { get; set; }
        #endregion
        #region Password
        [Tooltip("Password for the user"), EncryptedString]
        public string Password { get; set; }
        #endregion

        #region Hints
        public List<string> Hints { get; set; } = new List<string>();
        #endregion
    }
}
