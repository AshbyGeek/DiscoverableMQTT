using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscoverableMqtt.Tests.Probes
{
    [TestClass, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AppSettingsTest
    {
        [TestMethod]
        public void AppSettings_SaveReadSettings_Works()
        {
            // Get the temp file, but make sure it is empty
            var filePath = PrepareFile();

            // Generate, modify, and save settings
            var settings = new AppSettings
            {
                ApiId = int.MinValue,
                FilePath = filePath,
            };
            settings.SaveSettings();

            // Read the settings back
            var newSettings = new AppSettings()
            {
                FilePath = filePath,
            };
            newSettings.ReadFromFile();

            // Verify our settings match
            Assert.AreEqual(settings.ApiId, newSettings.ApiId);

            // Clean up our temp file
            PrepareFile();
        }
        
        /// <summary>
        /// Makes sure that the temporary json file doesn't exist and returns the path to it
        /// </summary>
        /// <returns></returns>
        private string PrepareFile()
        {
            var filePath = Path.Combine(Path.GetTempPath(), "MQTTExperiment.Tests.AppSettings.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return filePath;
        }

        [TestMethod]
        public void AppSettings_FilePath()
        {
            var settings = new AppSettings();
            settings.FilePath = "blah";
            Assert.AreEqual("blah", settings.FilePath);
        }

        [TestMethod]
        public void AppSettings_ResetToDefaults()
        {
            var settings = new AppSettings()
            {
                BrokerUrl = "dummy",
                DebugMode = true,
                FilePath = "bogus",
                ApiId = 12345566,
                ProbeDeviceName = "sugob",
                ProbeInterval = 5000,
                ProbeTopic = "gnitset"
            };
            settings.ResetToDefaults();

            var settings2 = new AppSettings();
            Assert.AreEqual(settings2.BrokerUrl, settings.BrokerUrl);
            Assert.AreEqual(settings2.DebugMode, settings.DebugMode);
            Assert.AreEqual(settings2.ApiId, settings.ApiId);
            Assert.AreEqual(settings2.ProbeDeviceName, settings.ProbeDeviceName);
            Assert.AreEqual(settings2.ProbeInterval, settings.ProbeInterval);
            Assert.AreEqual(settings2.ProbeTopic, settings.ProbeTopic);
        }

        [TestMethod]
        public void AppSettings_Json_InvalidSet()
        {
            var settings = new AppSettings();
            settings.Json = @" Broken }";

            // Make sure that invalid JSON can be handled without throwing an exception.
        }
    }
}
