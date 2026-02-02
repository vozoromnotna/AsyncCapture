using AsyncCapture.Core.Cameras;
using AsyncCapture.Ic4.Properties;
using ic4;

namespace AsyncCapture.Ic4.Sources;


public sealed class Ic4CS : CameraSource
{

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
        initStatusProp();
        //initImageSizeProp();
    }

    private void initStatusProp()
    {
        var statusProp = new Ic4StatusProp(_grabber);
        statusProp.IsHidden = true;
        Properties.Add(statusProp);
    }

    void initTrigger()
    {
        _grabber.DevicePropertyMap.TryFind(PropId.TriggerSource, out var triggerS);
         _grabber.DevicePropertyMap.TryFind(PropId.TriggerMode, out var triggerM);

         

         if (triggerS != null)
         {
             triggerS.Value = "Any";
         }

         if (triggerM != null)
         {
             triggerM.Value = "Off";
         }
        
        

        _grabber.DevicePropertyMap.TryFind(PropId.TriggerSoftware, out _trigger);
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
        _grabber.DevicePropertyMap.TryFind(PropId.ExposureTime, out var expProp);
        _grabber.DevicePropertyMap.TryFind(PropId.ExposureAuto, out var autoExpProp);
        
        if (expProp == null && autoExpProp == null)
            return;
        
        Ic4ExposureController exposureController = new(autoExpProp, expProp);

        if (autoExpProp != null) Properties.Add(new Ic4ExposureAutoProp(exposureController));
        
        Properties.Add(new Ic4ExposureTimeProp(exposureController));


        var fpsProp = _grabber.DevicePropertyMap.Find(PropId.AcquisitionFrameRate);

        fpsProp.Value = fpsProp.Maximum;

    }

    void initGainProp()
    {
        _grabber.DevicePropertyMap.TryFind(PropId.GainAuto, out var gainAuto);
        _grabber.DevicePropertyMap.TryFind(PropId.Gain, out var gain);

        if (gainAuto == null && gain == null)
            return;
        
        Ic4GainController gainController = new(gainAuto, gain);
        if (gainAuto != null)
        {
            gainAuto.Value = "Off";
            Properties.Add(new Ic4GainAutoProp(gainController));
        }

        Properties.Add(new Ic4GainProp(gainController));
    }

    /// <summary>
    /// _grabber нужно вызывать из потока создавшего его
    /// </summary>
    async void disposeSink()
    {
        try
        {
            //await _initDispatcher.InvokeAsync(_grabber.StreamStop);
            _grabber.StreamStop();
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
        var time = DateTime.Now;
        var sink = _grabber.Sink as QueueSink;
        if (sink == null)
            return;

        var res = sink.TryPopOutputBuffer(out var buffer);
        if (res)
        {
            var meta = new Dictionary<string, object>();
            meta["time"] = time;
            FillMeta(meta);
            
            await imageGetted(buffer.CreateOpenCvWrap(), meta);
        }
        buffer.Dispose();

        //_trigger.Execute();
    }

    private void FillMeta(Dictionary<string, object> meta)
    {
        var exposure = _grabber.DevicePropertyMap.Find(PropId.ExposureTime).Value;
        var gain = _grabber.DevicePropertyMap.Find(PropId.Gain).Value;

        meta["exposure"] = exposure;
        meta["gain"] = gain;
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
