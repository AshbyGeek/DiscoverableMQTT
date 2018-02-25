using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscoverableMqtt.Tests
{
    [TestClass]
    public class AppSettingsTest
    {
        [TestMethod]
        public void AppSettings_SaveReadSettings_Works()
        {
            // Get the temp file, but make sure it is empty
            var filePath = PrepareFile();

            // Generate, modify, and save settings
            var settings = new AppSettings();
            settings.Id = Guid.NewGuid();
            settings.SaveSettings(filePath);

            // Read the settings back
            var newSettings = new AppSettings();
            newSettings.ReadFromFile(filePath);

            // Verify our settings match
            Assert.AreEqual(settings.Id, newSettings.Id);

            // Clean up our temp file
            PrepareFile();
        }

        [TestMethod]
        public void AppSettings_PropertyChanged_Triggers()
        {
            var propertiesChanged = new List<string>();

            var settings = new AppSettings();
            settings.PropertyChanged += (s, e) => propertiesChanged.Add(e.PropertyName);

            settings.Id = Guid.NewGuid();

            Assert.IsTrue(propertiesChanged.Count == 1);
            Assert.IsTrue(propertiesChanged.Contains(nameof(settings.Id)));
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
    }
}
