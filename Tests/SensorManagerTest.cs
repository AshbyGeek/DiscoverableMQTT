using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DiscoverableMqtt.Tests
{
    [TestClass]
    public class SensorManagerTest
    {
        public AppSettings settings;
        public Fakes.FakeFactory moqFactory;
        public SensorManager manager;

        [TestInitialize]
        public void TestInit()
        {
            settings = new AppSettings()
            {
                ApiId = 42,
                Room = "blah/Moreblah/what",
                BrokerUrl = "hahahaha",
                MeasureInterval = 31415,
            };
            moqFactory = new Fakes.FakeFactory(true);
            manager = new SensorManager(settings, moqFactory.Object);
        }

        [TestMethod]
        public void SensorManager_Ctor()
        {
            moqFactory.Verify(x => x.CreateTempProbe(settings));
            VerifyUpdateFromSettings(false, settings);
            moqFactory.Probe.Verify(x => x.Start());
        }

        [TestMethod]
        public void SensorManager_UpdateFromSettings()
        {
            settings.ApiId = 24;
            settings.Room = "tahw/halberoM/halb";
            settings.BrokerUrl = "ahahahah";
            settings.MeasureInterval = 53141;

            manager.UpdateFromSettings();

            VerifyUpdateFromSettings(true, settings);
        }

        [TestMethod]
        public void SensorManager_ConfigListener_MsgReceived()
        {
            var json = @"{'" + nameof(AppSettings.Room) + "':'bogus', '" + nameof(AppSettings.BrokerUrl) + "':'sugob'}";
            var args = new MsgReceivedEventArgs()
            {
                Message = json,
                DupFlag = false,
                QosLevel = QosLevel.ExactlyOnce,
                Retain = false,
                Topic = "blah/Moreblah/what",
            };
            moqFactory.Listener.Raise(x => x.MsgReceived += null, args);

            Assert.AreEqual("bogus", settings.Room);
            Assert.AreEqual("sugob", settings.BrokerUrl);
            VerifyUpdateFromSettings(true, settings);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SensorManager_Probe_DataChanged(bool messengerIsConnected)
        {
            const float VAL = 15.5f;
            moqFactory.Messenger.Setup(x => x.IsConnected).Returns(messengerIsConnected);
            moqFactory.HelenApi.Setup(x => x.CreateDataMessage(settings.ApiId, VAL.ToString()))
                .Returns("hahahaha");

            moqFactory.Probe.Raise(x => x.DataChanged += null, new GenericEventArgs<float>(VAL));

            if (!messengerIsConnected)
            {
                moqFactory.Messenger.Verify(x => x.Connect());
            }
            moqFactory.Publisher.Verify(x => x.Publish("hahahaha"));
        }

        [TestMethod]
        public void SensorManager_Dispose()
        {
            manager.Dispose();

            moqFactory.Publisher.Verify(x => x.Dispose());
            moqFactory.Probe.Verify(x => x.Stop());
            moqFactory.Messenger.Verify(x => x.Disconnect());
        }

        private void VerifyUpdateFromSettings(bool expectDisposals, AppSettings settings)
        {
            moqFactory.Verify(x => x.CreateHelenApiInterface(It.IsAny<string>()));
            moqFactory.HelenApi.Verify(x => x.GetApiId(settings.Name, It.IsAny<int>()));
            moqFactory.HelenApi.Verify(x => x.GetBrokerUrl(It.IsAny<string>()));
            moqFactory.Verify(x => x.CreateMessenger(settings));
            moqFactory.Probe.VerifySet(x => x.MeasureInterval = settings.MeasureInterval);
            if (expectDisposals)
            {
                moqFactory.Publisher.Verify(x => x.Dispose(), Times.AtLeast(2));
            }
            moqFactory.Messenger.Verify(x => x.GetPublisher($"Rooms/{settings.Room}/Temperature", QosLevel.AtLeastOnce));
            moqFactory.Messenger.Verify(x => x.GetPublisher($"Devices/Configured/{settings.Name}", QosLevel.AtLeastOnce));
            moqFactory.Publisher.VerifySet(x => x.Retain = true);
            moqFactory.Publisher.Verify(x => x.Publish(It.IsAny<string>()));
            if (expectDisposals)
            {
                moqFactory.Listener.Verify(x => x.Dispose());
            }
            moqFactory.Messenger.Verify(x => x.GetListener("DeviceConfig/" + settings.Name, QosLevel.AtLeastOnce));
        }
    }
}
