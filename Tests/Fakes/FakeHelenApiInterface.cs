using System;
using System.Collections.Generic;
using System.Text;
using Moq;

namespace DiscoverableMqtt.Tests.Fakes
{
    public class FakeHelenApiInterface : Mock<IHelenApiInterface>
    {
        public FakeHelenApiInterface()
        {
            Setup(x => x.GetBrokerUrl(It.IsAny<string>()))
                .Returns((string defaultVal) => defaultVal);
            Setup(x => x.GetApiId(It.IsAny<string>(), It.IsAny<int>()))
                .Returns((string name, int defaultVal) => defaultVal);
            Setup(x => x.LocationMessageTopic).Returns("Linux/locationUpdates");
        }
    }
}
