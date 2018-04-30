using System;
using System.Collections.Generic;
using System.Text;
using Monitor = System.Threading.Monitor;
using System.Timers;

namespace DiscoverableMqtt.Probes
{
    public interface IAbstractProbe : IDisposable
    {
        double MeasureInterval { get; set; }
        AppSettings Settings { get; set; }

        event EventHandler<GenericEventArgs<float>> DataChanged;

        float GetCurrentData();
        void Start();
        void Stop();
    }

    public abstract class AbstractProbe : IAbstractProbe
    {
        /// <summary>
        /// Milliseconds between measurements
        /// </summary>
        public virtual double MeasureInterval
        {
            get => _Timer.Interval;
            set {
                if (value < 50)
                {
                    value = 50;
                }
                _Timer.Interval = value;
            }
        }

        private Timer _Timer = new Timer() { Interval = 500 };
        private float _CurVal;

        public AppSettings Settings { get; set; }

        public AbstractProbe()
        {
            _Timer.Elapsed += Timer_Elapsed;
            //GetNewVal();
        }

        public virtual void Start()
        {
            _Timer.Start();
        }

        public virtual void Stop()
        {
            _Timer.Stop();
        }

        public event EventHandler<GenericEventArgs<float>> DataChanged;


        public virtual float GetCurrentData() => _CurVal;

        object timerLock = new object();

        /// <summary>
        /// Override this method to customize what happens when the timer elapses.
        /// Usually overriding <see cref="GetNewVal"/> is good enough.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Timer_Elapsed(object sender, EventArgs e)
        {
            // Attempt to get a lock. If somebody else already has the lock, then we just exit.
            if (Monitor.TryEnter(timerLock))
            {
                try
                {
                    var prevEnabled = _Timer.Enabled;
                    _Timer.Stop();

                    _CurVal = GetNewVal();
                    DataChanged?.Invoke(this, new GenericEventArgs<float>(_CurVal));

                    _Timer.Enabled = prevEnabled;
                }
                finally
                {
                    // We must never, under any circumstances, forget to release the lock
                    Monitor.Exit(timerLock);
                }
            }
        }

        /// <summary>
        /// Implement this method to retrieve values from your temperature probe.
        /// Called ever time the time the timer interval elapses.
        /// </summary>
        public abstract float GetNewVal();

        public void Dispose() { }
    }
}
