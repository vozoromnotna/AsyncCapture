using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture
{
    internal class DisplayManager : ISink<Mat>, ISource<Mat>
    {
        public async Task PutImage(Mat image, Dictionary<string, object> meta)
        {
            foreach (var sink in _sinks)
            {
                await sink.PutImage(image, meta);
            }
        }
        List<ISink<Mat>> _sinks = new List<ISink<Mat>>();
        public void SetSink(ISink<Mat> sink)
        {
            _sinks.Add(sink);
        }

        public ISink<Mat> GetSink()
        {
            return _sinks[0];
        }
    }
}
