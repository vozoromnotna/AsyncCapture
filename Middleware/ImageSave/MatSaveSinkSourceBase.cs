using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Middleware.ImageSave;

public interface ISaveSinkSource
{
    public bool IsSave { get; set; }

}

public class MatSavedEventArgs : EventArgs
{
    public string Name { get; set; }
    public string DirectoryPath { get; set; }
    public bool IsSeries { get; set; }

    public string Filename { get; set; }
}

public abstract class MatSaverSinkSourceBase : MatSource, ISinkSource<Mat>, ISaveSinkSource
{
    public MatSaverSinkSourceBase(string name, string directoryPath)
    {
        _name = name;
        _directoryPath = directoryPath;
    }

    protected readonly string _name;
    protected string _directoryPath;
    protected string _recordDirectoryPath;

    public string RecordDirectoryPath
    {
        get => _recordDirectoryPath;
    }
    public string DirectoryPath 
    { 
        get => _directoryPath; 
        set => _directoryPath = value; 
    }

    public event EventHandler<MatSavedEventArgs> MatsSaved;

    protected bool _isSave;

    public bool IsSave
    {
        get => _isSave;
        set
        {
            if (value)
            {
                _recordDirectoryPath = GetRecordDirectoryPath();
                System.IO.Directory.CreateDirectory(_recordDirectoryPath);
                OnRecordStart();
            }
            else
            {
                OnRecordStop();
                MatsSaved?.Invoke(this, new MatSavedEventArgs { Name = _name, DirectoryPath = _recordDirectoryPath, IsSeries = true });
                _recordDirectoryPath = "";
            }
            _isSave = value;
        }
    }

    protected void RiseMatSaved(string name, string directoryPath, bool isSeries, string filename = "")
    {
        MatsSaved?.Invoke(this, new MatSavedEventArgs { Name = name, DirectoryPath = directoryPath, IsSeries = isSeries, Filename = filename });
    }

    protected abstract string GetRecordDirectoryPath();


    protected virtual void OnRecordStart()
    {

    }

    protected virtual void OnRecordStop()
    {

    }

    abstract protected Task SaveImage(Mat image, Dictionary<string, object> meta);

    private int _counter = 0;

    protected int _maxCount = 4;
    public int MaxCount
    {
        get => _maxCount;
        set => _maxCount = value;
    }
    public virtual async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        if (!_isSave)
        {
            await imageGetted(image, meta);
            return;
        }

        await SaveImage(image, meta);

        if (_counter >= MaxCount)
        {
            _counter = 0;
            await imageGetted(image, meta);
        }


        _counter++;

    }
}
