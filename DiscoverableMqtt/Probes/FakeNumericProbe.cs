using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace DiscoverableMqtt.Probes
{
    public class FakeNumericProbe : AbstractProbe
    {
        /// <summary>
        /// Seeding the random number with a constant will give us
        /// the same stream of numbers every time this class is initialized.
        /// This will be convenient for testing.
        /// </summary>
        private readonly Random _Gen = new Random(0);

        public const float CONV_FACTOR = 10.0f;
        public float MinVal { get; set; }
        public float MaxVal { get; set; }
        
        public FakeNumericProbe() { }

        public FakeNumericProbe(float minVal, float maxVal)
        {
            MinVal = minVal;
            MaxVal = maxVal;
        }
        
        public override float GetNewVal()
        {
            var tmpMin = MinVal * CONV_FACTOR;
            var tmpMax = MaxVal * CONV_FACTOR;
            return _Gen.Next((int)tmpMin, (int)tmpMax) / CONV_FACTOR;
        }
    }
}
