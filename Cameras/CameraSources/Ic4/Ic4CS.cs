using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using ic4;
using DirectShowLib;
using OpenCvSharp.Internal.Vectors;
using AsyncCapture.Cameras.CameraProperties.Ic4Properties;

namespace AsyncCapture.Cameras.CameraSources.Ic4;


public sealed class Ic4CS : CameraSource
{
    Dispatcher _initDispatcher = Dispatcher.CurrentDispatcher;

    Grabber _grabber;
    public Ic4CS()
    {
        Library.Init(LogLevel.Trace);

        var deviceList = DeviceEnum.Devices;

        if (deviceList.Count == 0)
        {
            throw new Exception("IC устройства не найдены");
        }

        _grabber = new Grabber();
        _grabber.DeviceOpen(deviceList[0]);


        var fps = _grabber.DevicePropertyMap.Find(PropId.AcquisitionFrameRate);
        fps.TrySetValue(30);

        var maxWidthProp = _grabber.DevicePropertyMap.Find(PropId.WidthMax);

        ImageMaxWidth = (int)maxWidthProp.Value;
        initProperties();

        //_trigger.Execute();
    }

    public Grabber Grabber { get => _grabber; }

    public int ImageMaxWidth { get; private set; }

    void initProperties()
    {
        initExposureProp();
        initTrigger();
        initGainProp();
        //initImageSizeProp();
    }

    void initTrigger()
    {
        var triggerS = _grabber.DevicePropertyMap.Find(PropId.TriggerSource);
        var triggerM = _grabber.DevicePropertyMap.Find(PropId.TriggerMode);

        triggerM.Value = "Off";
        triggerS.Value = "Any";

        _trigger = _grabber.DevicePropertyMap.Find(PropId.TriggerSoftware);
    }

    void initImageSizeProp()
    {
        var widthProp = _grabber.DevicePropertyMap.Find(PropId.Width);
        var heightProp = _grabber.DevicePropertyMap.Find(PropId.Height);

        PropInteger binningHProp = null;
        PropInteger binningVProp = null;
        var bH = _grabber.DevicePropertyMap.TryFind(PropId.BinningHorizontal, out binningHProp);
        var bV = _grabber.DevicePropertyMap.TryFind(PropId.BinningVertical, out binningVProp);

        var imageSizeController = new Ic4ImageSizeController(widthProp, heightProp, binningHProp, binningVProp);

        Properties.Add(new Ic4HeightProp(imageSizeController));
        Properties.Add(new Ic4WidthProp(imageSizeController));

        if (bH) Properties.Add(new Ic4BinningHorizontalProp(imageSizeController));
        if (bV) Properties.Add(new Ic4BinningVerticalProp(imageSizeController));

        imageSizeController.PropertyChanged += (sender, e) =>
        {
            restartSink();
        };
    }

    void initExposureProp()
    {
        var expProp = _grabber.DevicePropertyMap.Find(PropId.ExposureTime);
        var autoExpProp = _grabber.DevicePropertyMap.Find(PropId.ExposureAuto);
        Ic4ExposureController exposureController = new(autoExpProp, expProp);

        Properties.Add(new Ic4ExposureAutoProp(exposureController));
        Properties.Add(new Ic4ExposureTimeProp(exposureController));


        var fpsProp = _grabber.DevicePropertyMap.Find(PropId.AcquisitionFrameRate);

        fpsProp.Value = fpsProp.Maximum;

    }

    void initGainProp()
    {
        var gainAuto = (PropEnumeration)_grabber.DevicePropertyMap.Find("GainAuto");
        var gain = (PropFloat)_grabber.DevicePropertyMap.Find("Gain");

        gainAuto.Value = "Off";
        
        Ic4GainController gainController = new(gainAuto, gain);

        Properties.Add(new Ic4GainAutoProp(gainController));
        Properties.Add(new Ic4GainProp(gainController));
    }

    async void disposeSink()
    {
        //(_grabber.Sink as QueueSink).FramesQueued -= Sink_FramesQueued;
        //_grabber.Sink.Dispose();
        try
        {
            await _initDispatcher.InvokeAsync(_grabber.StreamStop);
        }
        catch
        {

        }

    }

    PropCommand _trigger;
    async Task initSink()
    {
        var sink = new QueueSink();
        sink.FramesQueued += Sink_FramesQueued1;
        _grabber.StreamSetup(sink, StreamSetupOption.AcquisitionStart);

    }

    private async void Sink_FramesQueued1(object? sender, QueueSinkEventArgs e)
    {
        var sink = _grabber.Sink as QueueSink;
        if (sink == null)
            return;

        var res = sink.TryPopOutputBuffer(out var buffer);
        if (res)
        {
            var meta = new Dictionary<string, object>();
            await imageGetted(buffer.CreateOpenCvWrap(), meta);
        }


        //_trigger.Execute();
    }

    void restartSink()
    {
        disposeSink();
        initSink();
    }

    bool _locker = false;

    public override void Dispose()
    {
        disposeSink();
        _grabber.Dispose();
    }

    public override void StartLive()
    {
        initSink();
        _isLive = true;
    }

    public override void StopLive()
    {
        disposeSink();
        _isLive = false;
    }

    public override string Name => "VIS";

    private bool _isLive;
    public override bool IsLive => _isLive;
}
