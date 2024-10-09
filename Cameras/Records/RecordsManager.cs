
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.Records;

public class RecordsManager
{
    private static ObservableCollection<PropertyRecord> _customRecords;

    public static ObservableCollection<PropertyRecord> GetCustomRecords()
    { 
        if (_customRecords == null)
        {
            try
            {
                string json;
                using (StreamReader reader = new StreamReader("custom_records.json"))
                {
                    json = reader.ReadToEnd();
                }
                _customRecords = JsonConvert.DeserializeObject<ObservableCollection<PropertyRecord>>(json);
            }
            catch
            {
                _customRecords = new ObservableCollection<PropertyRecord>();
            }

            subscribeCollection(_customRecords);
        }
        return _customRecords;
    }

    static void subscribeCollection(ObservableCollection<PropertyRecord> collection)
    {
        collection.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ((PropertyRecord)e.NewItems[0]).PropertyChanged += RecordsManager_PropertyChanged;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ((PropertyRecord)e.OldItems[0]).PropertyChanged -= RecordsManager_PropertyChanged;
                    break;
                
            }

            RecordsManager_PropertyChanged(null, null);
        };

        foreach (var item in collection)
        {
            item.PropertyChanged += RecordsManager_PropertyChanged;
        }
    }

    private static void RecordsManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var json = JsonConvert.SerializeObject(_customRecords);
        System.IO.File.WriteAllText("custom_records.json", json);
    }

    public static ObservableCollection<PropertyRecord> GetStaticRecords()
    {
        var records = new ObservableCollection<PropertyRecord>();

        return records;
    }
}
