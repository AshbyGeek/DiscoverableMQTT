using System;
using System.Collections.Generic;
using System.Text;

namespace DiscoverableMqtt
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Data;

        public GenericEventArgs(T data)
        {
            Data = data;
        }
    }

    public interface IDataSource<T>
    {
        event EventHandler<GenericEventArgs<T>> DataChanged;

        T GetCurrentData();
    }

    public interface IDataSink<T>
    {
        void SetCurrentData(T value);
    }
}
