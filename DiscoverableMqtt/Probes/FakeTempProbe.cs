using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace DiscoverableMqtt.Probes
{
    public class FakeTempProbe : AbstractTempProbe
    {
        /// <summary>
        /// Seeding the random number with a constant will give us
        /// the same stream of numbers every time this class is initialized.
        /// This will be convenient for testing.
        /// </summary>
        private Random _Gen = new Random(0);
        
        public FakeTempProbe() : base()  { }
        
        protected override float GetNewVal()
        {
            return _Gen.Next(600, 800) / 10.0f;
        }
    }
}
