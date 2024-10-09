using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AsyncCapture.Cameras.CameraProperties;
using AsyncCapture.Cameras.Records;

namespace AsyncCapture.Cameras.Filters;

public abstract class Filter : INotifyPropertyChanged, IPropertyContainer
{
    public Filter()
    {
        properties = new ObservableCollection<PropertyBase>();
        properties.CollectionChanged += (s, e) =>
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    (e.NewItems[0] as PropertyBase).PropertyChanged += Filter_PropertyChanged;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    (e.OldItems[0] as PropertyBase).PropertyChanged -= Filter_PropertyChanged;
                    break;
            }
        };
    }

    private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        SomePropertyChanged?.Invoke(sender, e);
    }

    public event EventHandler SomePropertyChanged;

    public abstract string Name { get; }
    public abstract string DisplayName { get; }

    private bool isOn;
    public bool IsOn 
    { 
        get=> isOn;
        set 
        { 
            isOn = value; 
            OnPropertyChanged();
            Filter_PropertyChanged(this, new PropertyChangedEventArgs("IsOn"));
        } 
    }
    public Mat FilterImage(Mat img)
    {
        if (IsOn)
        {
            return _FilterImage(img);
        }
        else
        {
            return img;
        }
    }

    protected abstract Mat _FilterImage(Mat img);

    protected ObservableCollection<PropertyBase> properties;
    public ObservableCollection<PropertyBase> Properties { get => properties; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    public virtual List<PropertyRecord> GetPropertyRecords()
    {
        var records = new List<PropertyRecord>();
        records.Add(new PropertyRecord { Name = $"{this.Name}_IsOn", Value = this.IsOn.ToString() }); 
        foreach (var prop in properties)
        {
            records.Add(prop.GetPropertyRecord(Name));
        }
        return records;
    }
}

