using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncCapture.Core.Cameras.Records;

namespace AsyncCapture.Core.Cameras.CameraProperties;

public abstract class PropertyBase : INotifyPropertyChanged
{
    public abstract string Name { get; }

    public abstract string DisplayName { get; }

    public virtual PropertyRecord GetPropertyRecord(string ownerName)
    {
        return new PropertyRecord { Name = this.Name, Value = "" };
        //return new PropertyRecord { Name = $"{ownerName}_{this.Name}", Value = ""};
    }

    public virtual void SetByPropertyRecord(PropertyRecord record)
    {

    }

    protected bool _isEnabled = true;
    virtual public bool IsEnabled 
    { 
        get => _isEnabled; 
        set 
        { 
            _isEnabled = value; 
            OnPropertyChanged(); 
        } 
    }

    public virtual void Update() { }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    public virtual string GetStringValue()
    {
        return "GetStringValue undefined";
    }
}
public abstract class Property<T> : PropertyBase
{
    protected T _minValue;

    protected T _maxValue;

    

    public T MinValue { get { return _minValue; } }
    public T MaxValue { get { return _maxValue; } }

    protected bool _isLogarithmic;
    public bool IsLogarithmic { get => _isLogarithmic; }

    protected T _value;

    protected bool _suppressNotifications = false;
    public T Value
    {
        get
        {
            var ret_value = GetValue();
            return ret_value;
        }
        set
        {
            if (!_isEnabled) 
                return;

            if (_suppressNotifications)
                return;

            this._value = value;
            SetValue(value);
            OnPropertyChanged();
        }
    }
    public virtual void SetValue(T val)
    {

    }

    public virtual T GetValue()
    {
        return _value;
    }

    public override PropertyRecord GetPropertyRecord(string ownerName)
    {
        return new PropertyRecord { Name = this.Name, Value = this.Value.ToString() };
    }

    public override void SetByPropertyRecord(PropertyRecord record)
    {
        var val = (T)Convert.ChangeType(record.Value, typeof(T));
        SetValue(val);
        _value = val;
        Update();
    }

    public override void Update()
    {
        if (_suppressNotifications)
            return;
        var oldEnabled = IsEnabled;
       // IsEnabled = true;
        _value = GetValue();
        OnPropertyChanged(nameof(Value));
        //IsEnabled = oldEnabled;
    }

    public override string GetStringValue()
    {
        return Value.ToString();
    }
}

interface IIncremented<T>
{
    T Increment { get; set; }
    T MinIncrement { get; }
}

public abstract class DoubleProperty : Property<double>, IIncremented<double>
{
    protected double increment;
    public double Increment
    {
        get => increment;
        set
        {
            if (value < MinIncrement)
            {
                increment = MinIncrement;
            }
            else
            {
                increment = value;
            }
            OnPropertyChanged();
        }
    }

    public virtual string FormatString => "0.00";

    public abstract double MinIncrement { get; }
}

public abstract class BoolProperty : Property<bool>
{

}

public abstract class uIntProperty : Property<uint>, IIncremented<uint>
{
    protected uint increment;
    public uint Increment 
    { 
        get => increment;
        set
        {
            if (value < MinIncrement)
            {
                increment = MinIncrement;

            }
            else
            {
                increment = value;
            }
            OnPropertyChanged();
        }
    }

    public abstract uint MinIncrement { get; }
  
}

public abstract class IntProperty : Property<int>, IIncremented<int>
{
    protected int increment;
    public int Increment
    {
        get => increment;
        set
        {
            if (value < MinIncrement)
            {
                increment = MinIncrement;

            }
            else
            {
                increment = value;
            }
            OnPropertyChanged();
        }
    }

    public abstract int MinIncrement { get; }

}




public abstract class ListProperty : PropertyBase
{

    protected ObservableCollection<string> values;
    public ObservableCollection<string> Values
    {
        get
        {
            var list = GetValues();
            return list;
        }
        protected set
        {
            values = value;
            OnPropertyChanged();
        }
    }

    protected int selectedIndex;
    public int SelectedIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            selectedIndex = value;
            SelectedItemChanged();
            OnPropertyChanged();
        }
    }

    protected abstract void SelectedItemChanged();

    protected virtual ObservableCollection<string> GetValues()
    {
        return values;
    }

    public ListProperty()
    {
        values = new ObservableCollection<string>();
    }

    public override PropertyRecord GetPropertyRecord(string ownerName)
    {
        return new PropertyRecord { Name = this.Name, Value = this.values[selectedIndex] };
    }

    public override void SetByPropertyRecord(PropertyRecord record)
    {
        var index = values.IndexOf(record.Value);
        SelectedIndex = index;
    }

    public override string GetStringValue()
    {
        return values[selectedIndex];
    }
}


