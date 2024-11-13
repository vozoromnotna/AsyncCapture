using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture
{
    public interface ISource<T>
    {
        public event EventHandler EndOfStream;
        void SetSink(ISink<T> sink);

        ISink<T> GetSink();
    }

    abstract public class SourceBase<T> : ISource<T>
    {
        protected ISink<T> _sink;

		public event EventHandler EndOfStream;

        protected void RiseEndOfStream()
        {
            EndOfStream?.Invoke(this, EventArgs.Empty);
        }

		public ISink<T> GetSink()
        {
            return _sink;
        }

        public void SetSink(ISink<T> sink)
        {
            _sink = sink;
        }

        protected virtual async Task imageGetted(T image, Dictionary<string, object> meta)
        {
            if (_sink != null) 
                await _sink?.PutImage(image, meta);
        }

    }

    abstract public class MatSource : SourceBase<Mat>
    {
        private bool _isLastCapture;
        private TaskCompletionSource _lastCapture;
        private Mat _last;
        private Dictionary<string, object> _lastMeta;
        protected override async Task imageGetted(Mat image, Dictionary<string, object> meta)
        {
            if (_isLastCapture)
            {
                _last = image.Clone();
                _lastMeta = meta;
                _lastCapture.SetResult();
                _isLastCapture = false;
            }
                
            await base.imageGetted(image, meta);
        }

        public async Task CaptureLast()
        {
            _lastCapture = new TaskCompletionSource();
            _isLastCapture = true;
            var timeout = Task.Delay(1000);
            await Task.WhenAny(_lastCapture.Task, timeout);
        }

        public async Task ReproccessLast()
        {
            using (_last)
            {
                await base.imageGetted(_last, _lastMeta);
            }
        }
    }

}
