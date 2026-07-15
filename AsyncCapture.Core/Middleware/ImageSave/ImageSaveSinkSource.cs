using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AsyncCapture.Core.Utils;
using Nito.AsyncEx;

namespace AsyncCapture.Core.Middleware.ImageSave;

public class ImageSaveSinkSource : MatSaverSinkSourceBase
{

    public ImageSaveSinkSource(string name, string directoryPath) : base(name, directoryPath)
    {

    }

    public bool IsSaveMeta { get; set; } = false;
    protected bool _saveSingle = false;

    private int _counter = 0;
    protected Mat _bufferMat;
    protected Dictionary<string, object> _bufferMeta;
    protected object _bufferLock = new object();
    protected SemaphoreSlim _bufferSemaphore = new SemaphoreSlim(1, 1);
    private TaskCompletionSource<(Mat Image, Dictionary<string, object> Meta)>? _singleFrameWaiter;
    public override async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        await _bufferSemaphore.WaitAsync();
        try
        {
            if (_bufferMat != null && !_bufferMat.Empty())
            {
                _bufferMat.Dispose();
            }
            _bufferMat = image.Clone();
            _bufferMeta = meta;
        }
        finally
        {
            _bufferSemaphore.Release();
        }

        CompleteSingleFrameWaiter(image, meta);

        if (IsSave && IsSaveMeta)
        {
            SaveMeta(meta);
        }
        
        await base.PutImage(image, meta);
        
        base.RiseMatSaved(_name, _directoryPath, false);
    }

    private void SaveMeta(Dictionary<string, object> meta)
    {
        meta ??= new Dictionary<string, object>();

        var metaFilenamePath = Path.Combine(_recordDirectoryPath, "meta.csv");
        bool fileExists = File.Exists(metaFilenamePath);

        using (var writer = new StreamWriter(metaFilenamePath, append: true))
        {
            if (!fileExists)
            {
                writer.WriteLine(string.Join(";", meta.Keys));
            }

            var formattedValues = meta.Values.Select(v => CsvEscape(MetaToString(v)).Replace(".", ","));
            writer.WriteLine(string.Join(";", formattedValues));
        }
    }

    private string MetaToString(object value)
    {
        if (value == null)
            return string.Empty;

        switch (value)
        {
            case DateTime dt:
                return dt.ToString("dd_MM_yyyy_HH_mm_ss_fff");

            case float f:
                return f.ToString("0.########", new System.Globalization.CultureInfo("ru-RU"));

            case double d:
                return d.ToString("0.########", new System.Globalization.CultureInfo("ru-RU"));

            default:
                return value.ToString();
        }
    }

    private string CsvEscape(string value)
    {
        // Если значение содержит спецсимволы — экранируем
        if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            value = "\"" + value + "\"";
        }
        return value;
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
        if (image == null || image.Empty())
            throw new InvalidOperationException("Кадр для сохранения еще не получен.");

        meta ??= new Dictionary<string, object>();

        string path = _recordDirectoryPath;

        if (String.IsNullOrEmpty(path))
            path = _directoryPath;

        var time = Helper.GetStringTime();
        if (meta.TryGetValue("time", out object timeRes) && timeRes is DateTime frameTime)
        {
            time = Helper.GetStringTime(frameTime);
        }

        var filename = await new ImageSaver(image.Clone(), camName: _name, path: path, saveFormat: SaveFormat, time: time).SaveAsync();
        LastFilename = filename;
    }


    public virtual void Single()
    {
        AsyncContext.Run(() => SingleAsync());
    }

    public virtual async Task SingleAsync(CancellationToken cancellationToken = default, Action? frameReceived = null)
    {
        System.IO.Directory.CreateDirectory(_directoryPath);
        _recordDirectoryPath = _directoryPath;

        var waiter = CreateSingleFrameWaiter();
        Mat? image = null;
        try
        {
            (image, var meta) = await waiter.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            frameReceived?.Invoke();
            await SaveImage(image, meta).ConfigureAwait(false);
        }
        finally
        {
            ClearSingleFrameWaiter(waiter);
            image?.Dispose();
        }

    }

    private TaskCompletionSource<(Mat Image, Dictionary<string, object> Meta)> CreateSingleFrameWaiter()
    {
        lock (_bufferLock)
        {
            if (_singleFrameWaiter != null && !_singleFrameWaiter.Task.IsCompleted)
                throw new InvalidOperationException("Сохранение кадра уже ожидает изображение.");

            _singleFrameWaiter = new TaskCompletionSource<(Mat Image, Dictionary<string, object> Meta)>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            return _singleFrameWaiter;
        }
    }

    private void CompleteSingleFrameWaiter(Mat image, Dictionary<string, object> meta)
    {
        TaskCompletionSource<(Mat Image, Dictionary<string, object> Meta)>? waiter;
        lock (_bufferLock)
        {
            waiter = _singleFrameWaiter;
            if (waiter == null || waiter.Task.IsCompleted)
                return;
        }

        var imageToSave = image.Clone();
        var metaToSave = meta == null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(meta);

        if (!waiter.TrySetResult((imageToSave, metaToSave)))
        {
            imageToSave.Dispose();
        }
    }

    private void ClearSingleFrameWaiter(TaskCompletionSource<(Mat Image, Dictionary<string, object> Meta)> waiter)
    {
        lock (_bufferLock)
        {
            if (ReferenceEquals(_singleFrameWaiter, waiter))
            {
                _singleFrameWaiter = null;
            }
        }
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
