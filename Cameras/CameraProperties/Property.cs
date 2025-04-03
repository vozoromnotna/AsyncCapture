using AsyncCapture.Cameras.Records;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraProperties;

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

public abstract class ButtonPlusMinusProperty : Property<string>
{
    public ButtonPlusMinusProperty()
    {
        _value = "";
    }
    RelayCommand plusCommand;
    public RelayCommand PlusCommand
    {
        get 
        {
            return plusCommand ?? (plusCommand = new RelayCommand(obj =>
            {
                PlusButton();
                OnPropertyChanged("Value");
            })); 
        }
    }

    RelayCommand minusCommand;
    public RelayCommand MinusCommand
    {
        get
        {
            return minusCommand ?? (minusCommand = new RelayCommand(obj =>
            {
                MinusButton();
                OnPropertyChanged("Value");
            }));
        }
    }

    RelayCommand plusMouseDownCommand;

    public RelayCommand PlusMouseDownCommand
    {
        get
        {
            return plusMouseDownCommand ?? (plusMouseDownCommand = new RelayCommand(obj => 
            {
                PlusMouseDown();
                OnPropertyChanged("Value");
            }));
        }
    }

    RelayCommand plusMouseUpCommand;

    public RelayCommand PlusMouseUpCommand
    {
        get
        {
            return plusMouseUpCommand ?? (plusMouseUpCommand = new RelayCommand(obj =>
            {
                PlusMouseUp();
                OnPropertyChanged("Value");
            }));
        }
    }

    RelayCommand minusMouseDownCommand;

    public RelayCommand MinusMouseDownCommand
    {
        get
        {
            return minusMouseDownCommand ?? (minusMouseDownCommand = new RelayCommand(obj =>
            {
                MinusMouseDown();
                OnPropertyChanged("Value");
            }));
        }
    }

    RelayCommand minusMouseUpCommand;

    public RelayCommand MinusMouseUpCommand
    {
        get
        {
            return minusMouseUpCommand ?? (minusMouseUpCommand = new RelayCommand(obj =>
            {
                MinusMouseUp();
                OnPropertyChanged("Value");
            }));
        }
    }

    protected bool _isMinusEnabled = true;
    public bool IsMinusEnabled
    {
        get => _isMinusEnabled;
        set
        {
            _isMinusEnabled = value;
            OnPropertyChanged();
        }
    }

    protected bool _isPlusEnabled = true;
    public bool IsPlusEnabled
    {
        get => _isPlusEnabled;
        set
        {
            _isPlusEnabled = value;
            OnPropertyChanged();
        }
    }

    public virtual void PlusButton()
    {

    }
    public virtual void MinusButton()
    {

    }

    public virtual void PlusMouseDown()
    {

    }

    public virtual void PlusMouseUp()
    {

    }

    public virtual void MinusMouseDown()
    {

    }

    public virtual void MinusMouseUp()
    {

    }

    public override PropertyRecord GetPropertyRecord(string ownerName)
    {
        return null;
    }

}

public abstract class PelcoButtonProperty : ButtonPlusMinusProperty
{

    RelayCommand setMinCommand;
    public RelayCommand SetMinCommand
    {
        get
        {
            return setMinCommand ?? (setMinCommand = new RelayCommand(obj =>
            {
                SetMin();
            }));
        }
    }

    RelayCommand setMaxCommand;
    public RelayCommand SetMaxCommand
    {
        get
        {
            return setMaxCommand ?? (setMaxCommand= new RelayCommand(obj =>
            {
                SetMax();
                
            }));
        }
    }

    protected int _boundarySetDelay = 10000;
    public virtual async Task SetMin()
    {
        IsEnabled = false;
        MinusMouseDownCommand.Execute(null);
        await Task.Delay(_boundarySetDelay);
        MinusMouseUpCommand.Execute(null);
        IsEnabled = true;
        OnPropertyChanged("Value");
    }

    public virtual async Task SetMax()
    {
        IsEnabled = false;
        PlusMouseDownCommand.Execute(null);
        await Task.Delay(_boundarySetDelay);
        PlusMouseUpCommand.Execute(null);
        IsEnabled = true;
        OnPropertyChanged("Value");
    }

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

public abstract class ButtonProperty : PropertyBase
{
    private RelayCommand buttonCommand;
    public RelayCommand ButtonCommand
    {
        get
        {
            return buttonCommand ?? (buttonCommand = new RelayCommand(obj =>
            {
                OnButtonClicked();
            })); ;
        }
    }
    abstract public void OnButtonClicked();

    public override PropertyRecord GetPropertyRecord(string ownerName)
    {
        return null;
    }
}
