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
        public Fakes.FakeHelenApiInterface moqHelenApiInterface;
        public SensorManager manager;

        [TestInitialize]
        public void TestInit()
        {
            settings = new AppSettings()
            {
                ApiId = 42,
                ProbeTopic = "blah/Moreblah/what",
                BrokerUrl = "hahahaha",
                ProbeInterval = 31415,
            };
            moqFactory = new Fakes.FakeFactory(true);
            moqHelenApiInterface = new Fakes.FakeHelenApiInterface();
            manager = new SensorManager(settings, moqFactory.Object, moqHelenApiInterface.Object);
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
            settings.ProbeTopic = "tahw/halberoM/halb";
            settings.BrokerUrl = "ahahahah";
            settings.ProbeInterval = 53141;

            manager.UpdateFromSettings();

            VerifyUpdateFromSettings(true, settings);
        }

        [TestMethod]
        public void SensorManager_ConfigListener_MsgReceived()
        {
            var json = @"{'ProbeTopic':'bogus', 'BrokerUrl':'sugob'}";
            var args = new MsgReceivedEventArgs()
            {
                Message = json,
                DupFlag = false,
                QosLevel = QosLevel.ExactlyOnce,
                Retain = false,
                Topic = "blah/Moreblah/what",
            };
            moqFactory.Listener.Raise(x => x.MsgReceived += null, (EventArgs)args);

            Assert.AreEqual("bogus", settings.ProbeTopic);
            Assert.AreEqual("sugob", settings.BrokerUrl);
            VerifyUpdateFromSettings(true, settings);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SensorManager_Probe_DataChanged(bool messengerIsConnected)
        {
            moqFactory.Messenger.Setup(x => x.IsConnected).Returns(messengerIsConnected);

            moqFactory.Probe.Raise(x => x.DataChanged += null, new GenericEventArgs<float>(15.5f));

            if (!messengerIsConnected)
            {
                moqFactory.Messenger.Verify(x => x.Connect());
            }
            moqFactory.Publisher.Verify(x => x.Publish((15.5f).ToString()));
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
            moqHelenApiInterface.Verify(x => x.GetApiId(settings));
            moqHelenApiInterface.Verify(x => x.GetBrokerUrl(settings));
            moqFactory.Verify(x => x.CreateMessenger(settings));
            if (expectDisposals)
            {
                moqFactory.Publisher.Verify(x => x.Dispose());
            }
            moqFactory.Messenger.Verify(x => x.GetPublisher(settings.ProbeTopic, QosLevel.AtLeastOnce));
            moqFactory.Probe.VerifySet(x => x.MeasureInterval = settings.ProbeInterval);
            if (expectDisposals)
            {
                moqFactory.Listener.Verify(x => x.Dispose());
            }
            moqFactory.Messenger.Verify(x => x.GetListener("DeviceConfig/" + settings.Name, QosLevel.AtLeastOnce));
        }
    }
}
