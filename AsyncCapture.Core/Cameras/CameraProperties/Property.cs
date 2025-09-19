using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncCapture.Core.Cameras.Records;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AsyncCapture.Core.Cameras.CameraProperties;

public abstract partial class PropertyBase : ObservableObject
{
    public abstract string Name { get; }

    public abstract string DisplayName { get; }

    public virtual PropertyRecord GetPropertyRecord()
    {
        return new PropertyRecord { Name = this.Name, Value = "" };
    }
    public abstract void SetByPropertyRecord(PropertyRecord record);

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

    public override string ToString()
    {
        return "ToString undefined";
    }
}
public abstract class Property<T> : PropertyBase
{
    public abstract T MinValue { get; }
    public abstract T MaxValue { get; }
    
    protected T _value;

    protected bool _suppressNotifications = false;
    public T Value
    {
        get => GetValue();
        set
        {
            if (!IsEnabled) 
                return;

            if (_suppressNotifications)
                return;

            _value = value;
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

    public override PropertyRecord GetPropertyRecord()
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

    public override string ToString()
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
    protected double _increment;
    public double Increment
    {
        get => _increment;
        set
        {
            if (value < MinIncrement)
            {
                _increment = MinIncrement;
            }
            else
            {
                _increment = value;
            }
            OnPropertyChanged();
        }
    }

    public virtual string FormatString => "0.00";

    public abstract double MinIncrement { get; }
}

public abstract class BoolProperty : Property<bool>
{
    public override bool MinValue { get => false; }
    public override bool MaxValue { get => true; }
}

public abstract class UintProperty : Property<uint>, IIncremented<uint>
{
    protected uint _increment;
    public uint Increment 
    { 
        get => _increment;
        set
        {
            if (value < MinIncrement)
            {
                _increment = MinIncrement;

            }
            else
            {
                _increment = value;
            }
            OnPropertyChanged();
        }
    }

    public abstract uint MinIncrement { get; }
  
}

public abstract class IntProperty : Property<int>, IIncremented<int>
{
    protected int _increment;
    public int Increment
    {
        get => _increment;
        set
        {
            if (value < MinIncrement)
            {
                _increment = MinIncrement;

            }
            else
            {
                _increment = value;
            }
            OnPropertyChanged();
        }
    }

    public abstract int MinIncrement { get; }

}




public abstract class ListProperty : PropertyBase
{

    protected ObservableCollection<string> _values;
    public ObservableCollection<string> Values
    {
        get
        {
            var list = GetValues();
            return list;
        }
        protected set
        {
            _values = value;
            OnPropertyChanged();
        }
    }

    protected int _selectedIndex;
    public int SelectedIndex
    {
        get
        {
            return _selectedIndex;
        }
        set
        {
            _selectedIndex = value;
            SelectedItemChanged();
            OnPropertyChanged();
        }
    }

    protected abstract void SelectedItemChanged();

    protected virtual ObservableCollection<string> GetValues()
    {
        return _values;
    }

    public ListProperty()
    {
        _values = new ObservableCollection<string>();
    }

    public override PropertyRecord GetPropertyRecord()
    {
        return new PropertyRecord { Name = this.Name, Value = this._values[_selectedIndex] };
    }

    public override void SetByPropertyRecord(PropertyRecord record)
    {
        var index = _values.IndexOf(record.Value);
        SelectedIndex = index;
    }

    public override string ToString()
    {
        return _values[_selectedIndex];
    }
}


