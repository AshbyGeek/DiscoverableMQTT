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
            Setup(x => x.GetBrokerUrl(It.IsAny<AppSettings>()))
                .Returns((AppSettings settings) => settings?.BrokerUrl ?? "");
            Setup(x => x.GetApiId(It.IsAny<AppSettings>()))
                .Returns((AppSettings settings) => settings?.ApiId ?? 0);
        }
    }
}
