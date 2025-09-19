using AsyncCapture.Core.Cameras.CameraProperties;

namespace AsyncCapture.Wpf;

public class PropSynchronizer : DoubleProperty
{
    private string _name;
    public override string Name => _name;

    private string _displayName;
    public override string DisplayName => _displayName;

    private List<PropertyBase> _propList = new List<PropertyBase>();


    public PropSynchronizer(string displayName, string name = "")
    {

        if (name == "")
            name = displayName.ToLower().Replace(" ", "_");

        _displayName = displayName;
        _name = name;

    }

    public override void SetValue(double val)
    {
        foreach( var property in _propList)
        {
            var type = property.GetType();
            
            var generic = type.BaseType.BaseType.GetGenericArguments()[0];
            var propToSet = type.GetProperty("Value");

            var valToSet = Convert.ChangeType(val, generic);

            propToSet.SetValue(property, valToSet);


        }
    }

    public override double GetValue()
    {
        var prop = _propList.First();
        var typeProp = prop.GetType().GetProperty("Value");
        return (double)Convert.ChangeType(typeProp.GetValue(prop), typeof(double));
    }

    public void AddProp(PropertyBase prop)
    {
        var minProp = prop.GetType().GetProperty("MinValue");
        var maxProp = prop.GetType().GetProperty("MaxValue");

        var minPropValue = (double)Convert.ChangeType(minProp.GetValue(prop), typeof(double));
        var maxPropValue = (double)Convert.ChangeType(maxProp.GetValue(prop), typeof(double));

        prop.PropertyChanged += Prop_PropertyChanged;

        _propList.Add(prop);

        if (_propList.Count == 1)
        {
            _minValue = minPropValue;
            _maxValue = maxPropValue;
            return;
        }
            
        if (_minValue < minPropValue)
            _minValue = minPropValue;

        if (_maxValue > maxPropValue)
            _maxValue = maxPropValue;
    }

    private void Prop_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged("Value");
    }

    public int Count
    {
        get => _propList.Count;
    }

    private double _minIncrement = 0.1;
    public override double MinIncrement => _minIncrement;
}

public class PelcoPropSynchronizer : PelcoButtonProperty
{
    private string _name;
    public override string Name => _name;

    private string _displayName;
    public override string DisplayName => _displayName;

    private List<PelcoButtonProperty> _propList = new List<PelcoButtonProperty>();

    public PelcoPropSynchronizer(string displayName, string name)
    {
        if (name == "")
            name = displayName.ToLower().Replace(" ", "_");

        _displayName = displayName;
        _name = name;
    }

    public void AddProp(PelcoButtonProperty prop)
    {
        prop.PropertyChanged += Prop_PropertyChanged;

        _propList.Add(prop);
    }

    private void Prop_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged("Value");
    }

    public override void PlusMouseDown()
    {
        foreach (var prop in _propList)
        {
            prop.PlusMouseDownCommand.Execute(null);
        }
    }

    public override void PlusMouseUp()
    {
        foreach (var prop in _propList)
        {
            prop.PlusMouseUpCommand.Execute(null);
        }
    }

    public override void MinusMouseDown()
    {
        foreach (var prop in _propList)
        {
            prop.MinusMouseDownCommand.Execute(null);
        }
    }

    public override void MinusMouseUp()
    {
        foreach (var prop in _propList)
        {
            prop.MinusMouseUpCommand.Execute(null);
        }
    }

    public override async Task SetMin()
    {
        IsEnabled = false;
        var awaitedTasks = new List<Task>();
        foreach (var prop in _propList)
        {
            awaitedTasks.Add(prop.SetMin());
        }
        await Task.WhenAll(awaitedTasks);
        IsEnabled = true;
    }

    public override async Task SetMax()
    {
        IsEnabled = false;
        var awaitedTasks = new List<Task>();
        foreach (var prop in _propList)
        {
            awaitedTasks.Add(prop.SetMax());
        }
        await Task.WhenAll(awaitedTasks);
        IsEnabled = true;
    }

    public int Count
    {
        get => _propList.Count;
    }
}


