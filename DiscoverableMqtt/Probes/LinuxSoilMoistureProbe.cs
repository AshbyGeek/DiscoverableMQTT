using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace DiscoverableMqtt.Probes
{
    public class LinuxSoilMoistureProbe : AbstractProbe, IDisposable
    {
        private const int CHAN_FREQ = 1_000_000;
        private const int CHAN_CONFIG_DIFF = 0b0000;   //0
        private const int CHAN_CONFIG_SINGLE = 0b1000; //8

        public LinuxSoilMoistureProbe()
        {
        }

        public override float GetNewVal()
        {
            //calibration settings
            var minVal = 0;
            var maxVal = 420;
            
            var tmpVal = GetAnalogValue(0);

            ConsoleExtensions.WriteDebugLocation($"Raw Moisture Value: {tmpVal}", 4);
            var percent = (tmpVal - minVal) / (float)(maxVal - minVal);
            return percent;
        }

        public int GetAnalogValue(int analogChannel)
        {
            int config = CHAN_CONFIG_SINGLE;

            var buffer = new byte[] { 1, 0, 0 };
            buffer[1] = (byte)((config + analogChannel) << 4);

            Pi.Spi.Channel0Frequency = CHAN_FREQ;
            var returnedBuffer = Pi.Spi.Channel0.SendReceive(buffer);
            int tmpVal = (returnedBuffer[1] & 0b0011) << 8; // Get the two most significant bits and shift them above the first byte
            tmpVal += returnedBuffer[2];    // Add the remaining byte

            return tmpVal;
        }
    }
}
