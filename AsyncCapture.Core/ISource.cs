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
        private readonly object _lastCaptureLock = new();
        private bool _isLastCapture;
        private TaskCompletionSource _lastCapture;
        private Mat _last;
        private Dictionary<string, object> _lastMeta;

        /// <summary>Opt in when the source reuses/disposes buffers and its last frame must remain replayable.</summary>
        protected virtual bool OwnLastCapture => false;
        protected virtual Mat RetainLastCapture(Mat image) => image;
        protected virtual void ReleaseLastCapture(Mat image) { }

        protected void ClearLastCapture()
        {
            if (!OwnLastCapture) return;
            Mat previous;
            lock (_lastCaptureLock) { previous = _last; _last = null; _lastMeta = null; }
            if (previous != null) ReleaseLastCapture(previous);
        }

        protected override async Task imageGetted(Mat image, Dictionary<string, object> meta)
        {
            Mat previous;
            TaskCompletionSource captureSignal = null;
            lock (_lastCaptureLock)
            {
                previous = _last;
                _last = OwnLastCapture ? RetainLastCapture(image) : image;
                _lastMeta = OwnLastCapture ? new Dictionary<string, object>(meta) : meta;
                if (_isLastCapture)
                {
                    _isLastCapture = false;
                    captureSignal = _lastCapture;
                }
            }
            captureSignal?.TrySetResult();
            if (OwnLastCapture && previous != null) ReleaseLastCapture(previous);
            await base.imageGetted(image, meta);
        }

        public async Task CaptureLast()
        {
            var capture = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_lastCaptureLock)
            {
                _lastCapture = capture;
                _isLastCapture = true;
            }
            await Task.WhenAny(capture.Task, Task.Delay(1000));
            lock (_lastCaptureLock)
            {
                if (ReferenceEquals(_lastCapture, capture) && !capture.Task.IsCompleted)
                {
                    _isLastCapture = false;
                    _lastCapture = null;
                }
            }
        }

        public async Task ReproccessLast()
        {
            Mat image;
            Dictionary<string, object> meta;
            var owned = OwnLastCapture;
            if (owned)
            {
                lock (_lastCaptureLock)
                {
                    if (_last == null || _lastMeta == null) throw new InvalidOperationException("No retained frame is available to reprocess.");
                    image = RetainLastCapture(_last);
                    meta = new Dictionary<string, object>(_lastMeta);
                }
            }
            else
            {
                image = _last;
                meta = _lastMeta is null
                    ? new Dictionary<string, object>(StringComparer.Ordinal)
                    : new Dictionary<string, object>(_lastMeta);
            }

            // Replayed retained frames are useful for preview/save, but they are
            // not new acquisitions and must never enter an attached recorder.
            meta ??= new Dictionary<string, object>(StringComparer.Ordinal);
            meta["capture.isReplay"] = true;

            try { await base.imageGetted(image, meta); }
            finally { if (owned && image != null) ReleaseLastCapture(image); }
        }
    }

}
