using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture
{
    public class PipeLineFabric<T>
    {
        ISource<T> _source;

        public ISource<T> Source { get => _source; }

        List<ISinkSource<T>> _middleware = new List<ISinkSource<T>>();

        ISink<T> _sink;

        public PipeLineFabric(ISource<T> source)
        {
            _source = source;
        }

        public void SetSource(ISource<T> source)
        {
            _source = source;
        }

        public void SetSink(ISink<T> sink)
        {
            _sink = sink;
        }

        public List<ISinkSource<T>> Middleware
        {
            get => _middleware;
        }

        public void CreatePipeLine()
        {
            if (_sink == null)
                throw new Exception("Нет конечной точки");

            if (_source == null)
                throw new Exception("Нет источника");

            if (_middleware.Count > 0)
            {
                var curSinkSource = _middleware.First();
                _source.SetSink(curSinkSource);

                for (int i = 1; i < _middleware.Count; i++)
                {
                    curSinkSource.SetSink(_middleware[i]);
                    curSinkSource = _middleware[i];
                }

                curSinkSource.SetSink(_sink);
            }
            else
            {
                _source.SetSink(_sink);
            }
        }
    }
}
