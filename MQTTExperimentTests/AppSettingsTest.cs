using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace MQTTExperiment.Tests
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
            var settings = AppSettings.GetSettings(filePath);
            settings.Name = "Bob";
            settings.SaveSettings(filePath);

            // Read the settings back
            var fileContents = File.ReadAllText(filePath);
            var newSettings = AppSettings.GetSettings(filePath);

            // Verify our settings match
            Assert.AreEqual(settings.Name, newSettings.Name);

            // Clean up our temp file
            PrepareFile();
        }

        [TestMethod]
        public void AppSettings_PropertyChanged_Triggers()
        {
            var propertiesChanged = new List<string>();

            var settings = new AppSettings();
            settings.PropertyChanged += (s, e) => propertiesChanged.Add(e.PropertyName);

            settings.Name = "New Name";

            Assert.IsTrue(propertiesChanged.Count == 1);
            Assert.IsTrue(propertiesChanged.Contains(nameof(settings.Name)));
        }

        [TestMethod]
        public void AppSettings_DefaultName_StartsWithUnNamed()
        {
            var filePath = PrepareFile();
            var settings = AppSettings.GetSettings(filePath);
            Assert.IsTrue(settings.Name.StartsWith("UnNamed-"));
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
