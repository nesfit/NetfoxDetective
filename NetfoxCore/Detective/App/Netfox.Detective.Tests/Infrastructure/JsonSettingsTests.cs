using System.IO;
using Netfox.Detective.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Netfox.Detective.Tests.Infrastructure
{
    [TestFixture]
    public class JsonSettingsTests
    {
        private string _path;

        [SetUp]
        public void Setup()
        {
            _path = Path.GetTempFileName();
            if (File.Exists(_path))
                File.Delete(_path); // because Path.GetTempFileName() creates the file... even though it is a getter and
                                    // it shouldn't technically have side effects
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_path);
        }

        [Test]
        public void NewSettingsTest()
        {
            var json = new NetfoxJsonSettings(_path);
            json.Save();
            Assert.IsTrue(File.Exists(_path));
            Assert.IsTrue(JsonConvert.DeserializeObject(File.ReadAllText(_path)) is JObject);
        }

        [Test]
        public void SaveTest()
        {
            var json = new NetfoxJsonSettings(_path);
            var val = json.ConnectionString;
            json.Save();

            json = new NetfoxJsonSettings(_path);
            Assert.AreEqual(val, json.ConnectionString);
            
            val = json.ConnectionString = val + "(updated)";
            json.Save();

            json = new NetfoxJsonSettings(_path);
            Assert.AreEqual(val, json.ConnectionString);
        }
    }
}