using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsyncCapture.Cameras.CameraProperties;

public class ExpBlank : DoubleProperty
{
    private AutoExpBlank auto_exp;
    public override bool IsEnabled { get { return !auto_exp.Value; }  }

    public override string Name => "Exposition";

    public override string DisplayName => "Экспозиция";

    public override double MinIncrement => 0.1;

    public ExpBlank(AutoExpBlank autoExpBlank)
    {
        _value = 10;
        _minValue = 0;
        _maxValue = 10;
        auto_exp = autoExpBlank;
        auto_exp.valueChanged += (bool x) => { OnPropertyChanged("IsEnabled"); };
        
    }
}

public class GainBlank : DoubleProperty
{
    public GainBlank()
    {
        _value = 20;
        _minValue = 0;
        _maxValue = 100;
    }

    public override string Name => "Gain";

    public override string DisplayName => "Усиление";

    public override double MinIncrement => 0.1;
}

public class IntBlank : IntProperty
{
    public IntBlank()
    {
        _value = 10;
        _minValue = 0;
        _maxValue = 50;
    }
    public override int MinIncrement => 1;

    public override string Name => "Test";

    public override string DisplayName => "Test Int";
}

public class DoubleBlank : DoubleProperty
{
    public DoubleBlank()
    {
        _value = 30;
        _minValue = 10;
        _maxValue = 100;
    }
    public override double MinIncrement => 0.1;

    public override string Name => "Test";

    public override string DisplayName => "Test Int";
}

public class AutoExpBlank : BoolProperty
{
    public override string Name => "Auto_Exposition";

    public override string DisplayName => "Авто. экспозиция";

    public event Action<bool> valueChanged;

    public override void SetValue(bool val)
    {
        base.SetValue(val);
        valueChanged?.Invoke(val);
    }
    public AutoExpBlank()
    {
        _value = true;
    }
}

public class ImgFormatBlank : ListProperty
{
    public ImgFormatBlank() : base()
    {
        values.Add("RGB");
        values.Add("Gray");
        SelectedIndex = 0;
    }

    public override string Name => "Format";

    public override string DisplayName => "Формат изображения";
}

public class DoubleToPecloWrapper : PelcoButtonProperty
{
    private string _name;
    public override string Name => _name;

    private string _displayName;
    public override string DisplayName => _displayName;

    DoubleProperty _property;

    double _increment;
    int _delay;

    double _virtualMin;
    double _virtualMax;
    public DoubleToPecloWrapper(DoubleProperty property, string name, string displayName, double virtualMin, double virtualMax, double increment = 0.1, int delay = 10)
    {
        _name = name;
        _displayName = displayName;
        _property = property;
        _virtualMin = virtualMin;
        _virtualMax = virtualMax;
        _increment = increment;
        _delay = delay;
        IsMinusEnabled = true;
        IsPlusEnabled = true;
    }

    CancellationTokenSource _cts;
    private void startIncrementation(double increment, int delay)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(async () =>
        {
            while(!token.IsCancellationRequested)
            {
                var delayTask = Task.Delay(delay);
                if (_property.Value + increment > _virtualMax)
                {
                    _property.Value = _virtualMax;
                    IsPlusEnabled = false;
                    break;
                }

                if (_property.Value + increment < _virtualMin)//тут тоже +, так как полагается что при нажатии минуса increment < 0
                {
                    _property.Value = _virtualMin;
                    IsMinusEnabled = false;
                    break;
                }


                IsPlusEnabled = true;
                IsMinusEnabled = true;
                _property.Value += increment;
                await delayTask;
            }
            
        }, token);
    }

    private void stopIncrementation()
    {
        _cts?.Cancel();
    }

    public override void MinusMouseDown()
    {
        startIncrementation(-1*_increment, _delay);
    }

    public override void MinusMouseUp()
    {
        stopIncrementation();
    }

    public override void PlusMouseDown()
    {
        startIncrementation(_increment, _delay);
    }

    public override void PlusMouseUp()
    {
        stopIncrementation();
    }

    public override async Task SetMax()
    {
        IsMinusEnabled = true;
        _property.Value = _virtualMax;
        IsPlusEnabled = false;
    }

    public override async Task SetMin()
    {
        IsPlusEnabled = true;
        _property.Value = _virtualMin;
        IsMinusEnabled = false;
    }
}
