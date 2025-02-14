using AsyncCapture.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
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


            await SaveImage(image, meta);

            _saveSingle = false;

            base.RiseMatSaved(_name, _directoryPath, false);

            return;

        }

        await base.PutImage(image, meta);

    }

    public SaveFormat SaveFormat { get; set; } = SaveFormat.BMP;

    protected override async Task SaveImage(Mat image, Dictionary<string, object> meta)
    {
        string path = _recordDirectoryPath;

        if (String.IsNullOrEmpty(path))
            path = _directoryPath;

        var time = Helper.GetStringTime();
        new ImageSaver(image.Clone(), camName: _name, path: path, saveFormat: SaveFormat, time: time).SaveAsync();
    }


    public virtual void Single()
    {
        System.IO.Directory.CreateDirectory(_directoryPath);
        _saveSingle = true;
    }

    protected override string GetRecordDirectoryPath()
    {
        var time = Helper.GetStringTime();

        string directoryPath = $"{_directoryPath}\\SER_{_name}_{time}\\";

        return directoryPath;
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}
