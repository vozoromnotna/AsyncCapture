using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Core
{
    public interface ISource<T>
    {
        public event EventHandler EndOfStream;
        void SetSink(ISink<T> sink);

        void Start();

        void Stop();

        Task WaitAsync();

        ISink<T> GetSink();
    }

    abstract public class SourceBase<T> : ISource<T>
    {
        protected ISink<T> _sink;

		public event EventHandler EndOfStream;

        protected TaskCompletionSource _endOfStreamTCS;

        protected void RiseEndOfStream()
        {
            EndOfStream?.Invoke(this, EventArgs.Empty);
            _endOfStreamTCS?.SetResult();
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

        public virtual void Start()
        {
            _endOfStreamTCS = new TaskCompletionSource();
        }
        public abstract void Stop();
        public async Task WaitAsync()
        {
            await _endOfStreamTCS.Task;
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
            _last = image;
            _lastMeta = meta;

            if (_isLastCapture)
            {
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
            await base.imageGetted(_last, _lastMeta);
        }
    }

}
