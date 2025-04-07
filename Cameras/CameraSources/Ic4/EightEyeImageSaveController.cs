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
        return $"{_saveController.GetDirectory()}\\{_name}\\";
    }

    protected override async Task SaveImage(Mat image, Dictionary<string, object> meta)
    {
        _recordDirectoryPath = GetRecordDirectoryPath();
        var time = _saveController.GetTime();
        new ImageSaver(image.Clone(), camName: _name, path: _recordDirectoryPath, saveFormat: SaveFormat, time: time).SaveAsync();
    }

    public override void Single()
    {
        _directoryPath = $"{_saveController.GetDirectory()}\\{_name}";
        System.IO.Directory.CreateDirectory(_directoryPath);
        _saveSingle = true;
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
        return _recordDirectoryPath;
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
            else if (_counter < _savers.Count - 1)
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

    protected string GetSingleDirectoryPath()
    {
        var time = Helper.GetStringTime();

        string directoryPath = $"{_directoryPath}\\IMG_{_name}_{time}\\";

        return directoryPath;
    }

    public override void Single()
    {
        _counter = 0;
        _recordDirectoryPath = GetSingleDirectoryPath();
        _saveSingle = true;
        foreach(var saver in _savers)
        {
            saver.Single();
        }
        _saveSingle = false;

        RiseMatSaved("VIS", _recordDirectoryPath, false);
    }
}
