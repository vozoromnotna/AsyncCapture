using Gst;
using Gst.App;
using Gst.Video;
using OpenCvSharp;
using Task = System.Threading.Tasks.Task;
using Clock = Gst.Clock;

namespace AsyncCapture.Gstreamer;

public class GStreamerSink
{
    private Pipeline _pipeline;
    private AppSrc _appSrc;
    private Clock _clock;
    public GStreamerSink() 
    {
        
        Gst.Application.Init();
        Gst.Debug.SetDefaultThreshold(DebugLevel.Fixme);
    }

    public void CreatePipeline(string pipelineDescription, VideoFormat videoFormat, uint width, uint height, string appSrcParams = "")
    {
        var videoInfo = new VideoInfo();
        videoInfo.SetFormat(videoFormat, (uint)width, (uint)height);
        
        _pipeline = Gst.Parse.Launch($"appsrc name=app-src format=time {appSrcParams}! {videoInfo.ToCaps()} !{pipelineDescription}") as Pipeline;

        _clock = Gst.SystemClock.Obtain();

        _appSrc = new AppSrc(_pipeline.GetByName("app-src").Handle);
        _appSrc["emit-signals"] = true;
    }

    private ulong _startTime;
    private ulong _prevTime;

    public async Task StartStreamAsync()
    {
        _pipeline.SetState(State.Playing);
        _startTime = _clock.Time;
        await Task.Run(() =>
        {
            while (true)
            {
                var msg = _pipeline.Bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);
                if (msg.Type == MessageType.Error)
                {
                    throw new Exception(msg.ParseErrorDetails().ToString());
                }
                if (msg.Type == MessageType.Eos) 
                {
                    break;
                }
            }
            
            _pipeline.SetState(State.Null);
        });
    }

    public void EndOfStream()
    {
        _appSrc.EndOfStream();
    }


    SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    public async Task PushMatToStream(Mat mat)
    {
        await _semaphore.WaitAsync();

        try
        {
            var nowTime = _clock.Time - _startTime;

            var bufferSize = mat.Width * mat.Height * mat.ElemSize();
            var buffer = new Gst.Buffer(null, (ulong)bufferSize, AllocationParams.Zero);

            buffer.Duration = nowTime - _prevTime;
            buffer.Dts = nowTime;
            buffer.Pts = nowTime;



            buffer.Map(out var mapWrite, MapFlags.Write);

            using (var writeMat = Mat.FromPixelData(mat.Height, mat.Width, mat.Type(), mapWrite.DataPtr))
            {
                mat.CopyTo(writeMat);
            };


            buffer.Unmap(mapWrite);

            _appSrc.PushBuffer(buffer);

            buffer.Dispose();
        }
        finally
        {
            _semaphore.Release();
        }
        
    }


}
