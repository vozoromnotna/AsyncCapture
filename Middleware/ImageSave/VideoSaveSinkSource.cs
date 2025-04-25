using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Threading;
using OpenCvSharp;
using System.Runtime.InteropServices;
using Gst.Video;
using System.Collections;
using AsyncCapture.Utils;

namespace AsyncCapture.Middleware.ImageSave;

public class VideoSaveSinkSource : MatSaverSinkSourceBase
{
    private GStreamerSink _gstStream;
    public VideoSaveSinkSource(string name, string directoryPath) : base(name, directoryPath)
    {

    }

    protected override async Task SaveImage(Mat image, Dictionary<string, object> meta)
    {
        await _gstStream.PushMatToStream(image);
    }

    private OpenCvSharp.Size? _size;
    public override Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        if (_size == null)
        {
            _size = new OpenCvSharp.Size(image.Width, image.Height);
        }

        return base.PutImage(image, meta);
    }

    private string _fileformat = ".avi";
    protected override string GetRecordDirectoryPath()
    {
        System.IO.Directory.CreateDirectory(_directoryPath);
        return _directoryPath;
    }

    private Task _streamTask;

    public string LastFilename { get; private set; }
    protected override void OnRecordStart()
    {
        _gstStream = new GStreamerSink();

        var time = Helper.GetStringTime();

        LastFilename = $"VID_{_name}_{time}{_fileformat}";

        var fullPath = Path.Combine(_directoryPath, LastFilename).Replace('\\', '/');

        //mp4 h265 var pipelineDescription = $"videorate ! video/x-raw,framerate=30/1 ! videoconvert ! queue ! qsvh265enc ! h265parse ! mp4mux ! filesink location={formattedPath}{filename}";
        //mp4 h264 var pipelineDescription = $"videorate ! video/x-raw,framerate=30/1 ! videoconvert ! queue ! qsvh264enc ! mp4mux ! filesink location={formattedPath}{filename}";

        var pipelineDescription = $"videorate ! video/x-raw,framerate=30/1 ! videoconvert ! queue ! avimux ! filesink location={fullPath}";
        _gstStream.CreatePipeline(pipelineDescription, VideoFormat.Bgr, (uint)_size.Value.Width, (uint)_size.Value.Height);

        _streamTask = _gstStream.StartStreamAsync();
    }
    protected override void OnRecordStop()
    {
        _gstStream.EndOfStream();
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}
