using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;

namespace AsyncCapture.Wpf.Hist;

public class ObservablePoint
{
    public ObservablePoint(int x, int y) 
    {
        _x = x; _y = y;
    }

    public event EventHandler<PointChangedEventArgs> PointChanged;

    public int _x;
    public int _y;

    public int X 
    { 
        get => _x; 
        set 
        { 
            var oldValue = _x;
            _x = value;
            PointChanged(this, new PointChangedEventArgs(new Point(oldValue, _y), new Point(_x, _y)));
        } 
    }

    public int Y
    {
        get => _y;
        set
        {
            var oldValue = _y;
            _y = value;
            PointChanged(this, new PointChangedEventArgs(new Point(_x, oldValue), new Point(_x, _y)));
        }
    }

    public void SetXY(int x, int y)
    {
        var oldX = _x;
        var oldY = _y;
        _x = x;
        _y = y;
        PointChanged(this, new PointChangedEventArgs(new Point(oldX, oldY), new Point(_x, _y)));
    }
}

public class PointChangedEventArgs : EventArgs
{
    public Point OldValue { get; }
    public Point NewValue { get; }

    public PointChangedEventArgs(Point oldVal, Point newVal) 
    {
        OldValue = oldVal;
        NewValue = newVal;
    }

}

public class ObservablePointCollection : ObservableCollection<ObservablePoint>
{
    public event EventHandler<PointChangedEventArgs> PointChanged;
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                (e.NewItems[0] as ObservablePoint).PointChanged += OnPointChanged;
                break;
            case NotifyCollectionChangedAction.Remove:
                (e.OldItems[0] as ObservablePoint).PointChanged -= OnPointChanged;
                break;
        }
    }

    private void OnPointChanged(object sender, PointChangedEventArgs e)
    {
        PointChanged?.Invoke(sender, e);
    }

}

