using AsyncCapture.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace AsyncCapture.Middleware.ImageSave;

public class ImageSaveSinkSource : MatSaverSinkSourceBase
{

    public ImageSaveSinkSource(string name, string directoryPath) : base(name, directoryPath)
    {

    }

    protected bool _saveSingle = false;

    private int _counter = 0;
    public override async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        if (!_isSave)
        {
            if (!_saveSingle)
            {
                await imageGetted(image, meta);
                return;
            }

            _saveSingle = false;

            await SaveImage(image, meta);

            base.RiseMatSaved(_name, _directoryPath, false);

            return;

        }

        await base.PutImage(image, meta);

    }

    public SaveFormat SaveFormat { get; set; } = SaveFormat.BMP;

    object filenameLocker = new object();
    private string _lastFilename;
    public string LastFilename 
    {
        get => _lastFilename;
        private set
        {
            _lastFilename = value;
        }
    }

    protected override async Task SaveImage(Mat image, Dictionary<string, object> meta)
    {
        string path = _recordDirectoryPath;

        if (String.IsNullOrEmpty(path))
            path = _directoryPath;

        var time = Helper.GetStringTime();
        var filename = await new ImageSaver(image.Clone(), camName: _name, path: path, saveFormat: SaveFormat, time: time).SaveAsync();
        LastFilename = filename;
    }


    public virtual void Single()
    {
        System.IO.Directory.CreateDirectory(_directoryPath);
        _saveSingle = true;
    }

    protected override string GetRecordDirectoryPath()
    {
        var time = Helper.GetStringTime();

        var directoryName = $"SER_{_name}_{time}";
        string directoryPath = Path.Combine(_directoryPath, directoryName);

        return directoryPath;
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}
