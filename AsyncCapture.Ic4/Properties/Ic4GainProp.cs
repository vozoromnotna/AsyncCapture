using System.ComponentModel;
using System.Runtime.CompilerServices;
using AsyncCapture.Core.Cameras.CameraProperties;
using ic4;

namespace AsyncCapture.Ic4.Properties;

public class Ic4GainController : INotifyPropertyChanged
{
    private readonly PropEnumeration _autoGainProp;
    private readonly PropFloat _gainProp;
    public Ic4GainController(PropEnumeration autoGainProp, PropFloat gainProp)
    {
        _autoGainProp = autoGainProp;
        _gainProp = gainProp;

    }

    public bool IsAuto
    {
        get => _autoGainProp?.Value == "Continuous";
        set
        {
            if (_autoGainProp != null)
            {
                _autoGainProp.Value = value ? "Continuous" : "Off";
            }
            OnPropertyChanged();
        }
    }

    public double Gain
    {
        get => _gainProp.Value;
        set
        {
            _gainProp.Value = value;
            OnPropertyChanged();
        }
    }

    public double GainIncrement => 1;

    public (double, double) GainBounds => (_gainProp.Minimum, _gainProp.Maximum);

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}

public class Ic4GainProp : DoubleProperty
{
    public override double MinIncrement => _gainController.GainIncrement;

    public override string Name => "Gain";

    public override string DisplayName => "Усиление";

    private readonly Ic4GainController _gainController;

    private CancellationTokenSource _gainUpdateCts;
    public Ic4GainProp(Ic4GainController gainController)
    {
        _gainController = gainController;
        _gainController.PropertyChanged += (e, sender) =>
        {
            OnPropertyChanged("Value");
            OnPropertyChanged("IsEnabled");
            enabledCheck();
        };

        _value = gainController.Gain;

        (_minValue, _maxValue) = _gainController.GainBounds;

        enabledCheck();
    }

    private void enabledCheck()
    {
        if (!IsEnabled)
        {
            _gainUpdateCts = new CancellationTokenSource();
            var token = _gainUpdateCts.Token;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    OnPropertyChanged("Value");
                    await Task.Delay(500);
                }

            }, token);
        }
        else
        {
            _gainUpdateCts?.Cancel();
        }
    }

    public override bool IsEnabled { get => !_gainController.IsAuto; }

    public override double GetValue()
    {
        return _gainController.Gain;
    }
    public override void SetValue(double val)
    {
        _gainController.Gain = val;
    }
}

public class Ic4GainAutoProp : BoolProperty
{
    public override string Name => "Auto_Gain";

    public override string DisplayName => "Авто усиление";

    private readonly Ic4GainController _gainController;
    public Ic4GainAutoProp(Ic4GainController gainController)
    {
        _gainController = gainController;
    }

    public override bool GetValue()
    {
        return _gainController.IsAuto;
    }
    public override void SetValue(bool val)
    {
        _gainController.IsAuto = val;
    }
}
