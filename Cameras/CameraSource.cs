using AsyncCapture;
using AsyncCapture.Cameras.CameraProperties;
using AsyncCapture.Cameras.Records;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsyncCapture.Cameras;

abstract public class CameraSource : MatSource, INotifyPropertyChanged, IPropertyContainer, IDisposable
{

    ObservableCollection<PropertyBase> _properties;
    public ObservableCollection<PropertyBase> Properties { get => _properties; }
    public abstract string Name { get; }

    public CameraSource()
    {
        _properties = new ObservableCollection<PropertyBase>();
        _properties.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    (e.NewItems[0] as PropertyBase).PropertyChanged += Camera_OnPropertyChanged;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    (e.OldItems[0] as PropertyBase).PropertyChanged -= Camera_OnPropertyChanged;
                    break;
            }
        };
    }

    public event EventHandler SomePropertyChanged;
    private void Camera_OnPropertyChanged(object sender, EventArgs e)
    {
        SomePropertyChanged?.Invoke(sender, e);

    }

    public event PropertyChangedEventHandler PropertyChanged;
    

    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    public List<PropertyRecord> GetPropertyRecords()
    {
        var camRecord = new List<PropertyRecord>();
        foreach (var property in _properties)
        {
            var propToRecord = property.GetPropertyRecord(Name);
            if (propToRecord != null)
                camRecord.Add(propToRecord);
        }
        return camRecord;
    }

    public abstract bool IsLive {  get; }
    public abstract void StartLive();

    public abstract void StopLive();

    public abstract void Dispose();
}
