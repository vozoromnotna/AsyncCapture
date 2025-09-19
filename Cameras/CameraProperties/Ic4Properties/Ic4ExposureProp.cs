using ic4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties.Ic4Properties;

public class Ic4ExposureController : INotifyPropertyChanged
{
    private readonly PropEnumeration _autoExpProp;
    private readonly PropFloat _expTimeProp;
    public Ic4ExposureController(PropEnumeration autoExpProp, PropFloat expTimeProp) 
    {
        _autoExpProp = autoExpProp;
        _expTimeProp = expTimeProp;
        
    }

    public bool IsAuto
    {
        get => _autoExpProp?.Value == "Continuous";
        set
        {
            if (_autoExpProp != null)
            {
                _autoExpProp.Value = value ? "Continuous" : "Off";
            }
            OnPropertyChanged();
        }
    }

    public double ExposureTime
    {
        get => _expTimeProp.Value;
        set
        {
            _expTimeProp.Value = value;
            OnPropertyChanged();
        }
    }

    public double ExposureTimeIncrement => 1;

    public (double, double) ExposureTimeBounds => (_expTimeProp.Minimum, _expTimeProp.Maximum);

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}

public class Ic4ExposureTimeProp : DoubleProperty
{
    public override double MinIncrement => _exposureController.ExposureTimeIncrement * (_isMilliseconds ? 0.001 : 1);

    public override string Name => "Exposure_Time";

    private bool _isMilliseconds = false;
    public bool IsMilliseconds
    {
        get => _isMilliseconds;
        set
        {
            _isMilliseconds = value;
            (_minValue, _maxValue) = _exposureController.ExposureTimeBounds;
            if (IsMilliseconds)
            {
                _minValue = _minValue * 0.001;
                _maxValue = _maxValue * 0.001;
            }
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Value));
        }
    }
        

    public override string FormatString => (_isMilliseconds ? "0.0000" : "0");
    public override string DisplayName => "Экспозиция, " + (_isMilliseconds ? "мс" : "мкс");

    private readonly Ic4ExposureController _exposureController;

    private CancellationTokenSource _exposureUpdateCts;
    public Ic4ExposureTimeProp(Ic4ExposureController exposureController)
    {
        _isLogarithmic = true;
        _exposureController = exposureController;
        _exposureController.PropertyChanged += (e, sender) => 
        { 
            OnPropertyChanged("Value"); 
            OnPropertyChanged("IsEnabled");
            enabledCheck();
        };
        
        _value = exposureController.ExposureTime;

        (_minValue, _maxValue) = _exposureController.ExposureTimeBounds;

        enabledCheck();
    }

    private void enabledCheck()
    {
        if (!IsEnabled)
        {
            _exposureUpdateCts = new CancellationTokenSource();
            var token = _exposureUpdateCts.Token;
            Task.Run(async () => { 
                while(!token.IsCancellationRequested) 
                {
                    OnPropertyChanged("Value");
                    await Task.Delay(500);
                }
                
            }, token);
        }
        else
        {
            _exposureUpdateCts?.Cancel();
        }
    }

    public override bool IsEnabled { get => !_exposureController.IsAuto; }

    public override double GetValue()
    {
        var expTime = _exposureController.ExposureTime;
        if (IsMilliseconds)
            expTime *= 0.001;
        return expTime;
    }
    public override void SetValue(double val)
    {
        var expTime = val;
        if (IsMilliseconds)
            expTime *= 1000;

        _exposureController.ExposureTime = expTime;
    }
}

public class Ic4ExposureAutoProp : BoolProperty
{
    public override string Name => "Auto_Exposure";

    public override string DisplayName => "Авто экспозиция";

    private readonly Ic4ExposureController _exposureController;
    public Ic4ExposureAutoProp(Ic4ExposureController exposureController)
    {
        _exposureController = exposureController;
    }

    public override bool GetValue()
    {
        return _exposureController.IsAuto;
    }
    public override void SetValue(bool val)
    {
        _exposureController.IsAuto = val;
    }
}
