using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace DiscoverableMqtt
{
    public class FakeTempProbe : IDataSource<float>
    {
        /// <summary>
        /// Milliseconds between measurements
        /// </summary>
        public double MeasureInterval
        {
            get => _Timer.Interval;
            set => _Timer.Interval = value;
        }

        private Timer _Timer = new Timer() { Interval = 500 };
        private Random _Gen = new Random(0); // Yes this means I'll get this same sequence every time. This is good for testing.
        private float _CurVal;

        public FakeTempProbe()
        {
            _Timer.Elapsed += Timer_Elapsed;
            GetNewVal();
        }

        public void Start()
        {
            _Timer.Start();
        }

        public void Stop()
        {
            _Timer.Stop();
        }

        public event EventHandler<GenericEventArgs<float>> DataChanged;

        public float GetCurrentData() => _CurVal;

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            GetNewVal();
            DataChanged.Invoke(this, new GenericEventArgs<float>(_CurVal));
        }

        private void GetNewVal()
        {
            _CurVal = _Gen.Next(600, 800) / 10.0f;
        }
    }
}
