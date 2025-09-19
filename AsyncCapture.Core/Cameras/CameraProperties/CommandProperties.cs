using AsyncCapture.Core.Cameras.Records;
using CommunityToolkit.Mvvm.Input;

namespace AsyncCapture.Core.Cameras.CameraProperties;

public abstract class ButtonProperty : PropertyBase
{
    private RelayCommand buttonCommand;
    public RelayCommand ButtonCommand
    {
        get
        {
            return buttonCommand ?? (buttonCommand = new RelayCommand(() =>
            {
                OnButtonClicked();
            }));
        }
    }
    abstract public void OnButtonClicked();

    public override PropertyRecord GetPropertyRecord()
    {
        return null;
    }

    public override void SetByPropertyRecord(PropertyRecord record)
    {
        return;
    }
}

public abstract class PelcoButtonProperty : ButtonPlusMinusProperty
{

    RelayCommand setMinCommand;
    public RelayCommand SetMinCommand
    {
        get
        {
            return setMinCommand ?? (setMinCommand = new RelayCommand(() =>
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
            return setMaxCommand ?? (setMaxCommand= new RelayCommand(() =>
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

public abstract class ButtonPlusMinusProperty : Property<string>
{
    public override string MaxValue { get; }

    public override string MinValue { get; }

    public ButtonPlusMinusProperty()
    {
        _value = "";
    }
    RelayCommand plusCommand;
    public RelayCommand PlusCommand
    {
        get 
        {
            return plusCommand ?? (plusCommand = new RelayCommand(() =>
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
            return minusCommand ?? (minusCommand = new RelayCommand(() =>
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
            return plusMouseDownCommand ?? (plusMouseDownCommand = new RelayCommand(() => 
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
            return plusMouseUpCommand ?? (plusMouseUpCommand = new RelayCommand(() =>
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
            return minusMouseDownCommand ?? (minusMouseDownCommand = new RelayCommand(() =>
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
            return minusMouseUpCommand ?? (minusMouseUpCommand = new RelayCommand(() =>
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

    public override PropertyRecord GetPropertyRecord()
    {
        return null;
    }

    public override void SetByPropertyRecord(PropertyRecord record)
    {
        
    }
}
