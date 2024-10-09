using AsyncCapture.Middleware;
using AsyncCapture.Middleware.ImageSave;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncCapture.Utils;

namespace AsyncCapture.Cameras.CameraSources.Ic4;

public class EightEyeImageSaverSink : ImageSaveSinkSource
{
    private readonly EightEyeImageSaveController _saveController;
    public EightEyeImageSaverSink(EightEyeImageSaveController saveController, string name, string directoryPath) : base(name, directoryPath)
    {
        _saveController = saveController;
    }

    protected override string GetRecordDirectoryPath()
    {
        return $"{_saveController.GetDirectory()}{_name}\\";
    }

    protected override async Task SaveImage(Mat image, Dictionary<string, object> meta)
    {

        var time = _saveController.GetTime();

        new ImageSaver(image.Clone(), camName: _name, path: _directoryPath, saveFormat: SaveFormat, time: time).SaveAsync();
    }
}
public class EightEyeImageSaveController : ImageSaveSinkSource
{
    public EightEyeImageSaveController(string name, string directoryPath) : base(name, directoryPath)
    {
    }
    private readonly List<ImageSaveSinkSource> _savers = new();


    protected override void OnRecordStart()
    {
        foreach (var saver in _savers)
        {
            saver.IsSave = true;
        }
    }

    protected override void OnRecordStop()
    {
        foreach (var saver in _savers)
        {
            saver.IsSave = false;
        }
    }


    public void AddSaveSink(ImageSaveSinkSource imageSave)
    {
        _savers.Add(imageSave);
    }

    public string GetDirectory()
    {
        return _directoryPath;
    }

    int _counter = 0;
    object _lock = new object();
    string _saveTime;
    public string GetTime()
    {
        lock(_lock)
        {
            if (_counter == 0)
            {
                _saveTime = Helper.GetStringTime();
                _counter++;
            }
            else if (_counter < _savers.Count)
            {
                _counter++;
            }
            else
            {
                _counter = 0;
            }

            return _saveTime;
        }
        
    }

    public void Single()
    {
        _counter = 0;
        foreach(var saver in _savers)
        {
            saver.Single();
        }
    }
}
