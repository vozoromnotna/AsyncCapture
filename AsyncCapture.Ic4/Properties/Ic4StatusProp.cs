using AsyncCapture.Core.Cameras.CameraProperties;
using ic4;

namespace AsyncCapture.Ic4.Properties;

public class Ic4StatusProp : BoolProperty
{
    public override string Name { get => "Ic4StatusProp"; }
    public override string DisplayName { get => "Статус"; }

    private readonly Grabber _grabber;
    
    private Task _statusCheckTask;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    public Ic4StatusProp(Grabber grabber)
    {
        _grabber = grabber;
        var token = _cts.Token;
        _statusCheckTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000);
                OnPropertyChanged(nameof(Value));
            }
        }, token);

    }

    public override bool GetValue()
    {
        return _grabber.IsDeviceOpen && _grabber.IsDeviceValid && _grabber.IsStreaming && _grabber.IsAcquisitionActive;
    }
}