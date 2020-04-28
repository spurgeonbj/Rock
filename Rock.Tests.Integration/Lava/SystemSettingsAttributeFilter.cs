using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class SystemSettingsAttributeFilter
    {
        /// <summary>
        /// Fetches a known SystemSetting value from the database for comparisson.
        /// </summary>
        [TestMethod]
        [Ignore( "Lava is not returning the correct value from the database because the Strainer method (Attribute) is not available.  Probably missing some necessary configuration steps?" )]
        public void FetchSystemSettingValue()
        {
            var mergeFields = new Dictionary<string, object>();
            string testString = "{{ 'SystemSetting' | Attribute:'core_GenderAutoFillConfidence' }}";
            string expectedResult = "99.9"; // This is the default amount set in the migration when the setting is created.
            var resultValue = testString.ResolveMergeFields( mergeFields );
            Assert.That.AreEqual( expectedResult, resultValue );
        }
    }
}
