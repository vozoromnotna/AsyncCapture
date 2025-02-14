using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Middleware;

public class WriteTextSinkSource : MatSource, ISinkSource<Mat>
{
    protected string _text;
    protected OpenCvSharp.Point _position;
    protected double _fontScale;
    protected int _thickness;
    protected Scalar _color;
    public WriteTextSinkSource(string text, OpenCvSharp.Point position, double fontScale, int thickness = 1, Scalar? color = null)
    {
        _text = text;
        _position = position;
        _fontScale = fontScale;
        _thickness = thickness;
        _color = color == null ? Scalar.White : (Scalar)color;
    }

    private bool _isOn = true;

    public bool IsOn { get => _isOn; set => _isOn = value; }

    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        if (_isOn)
        {
            writeTextOnMat(image, getTextToWrite());
        }
        
        await imageGetted(image, meta);
    }

    protected virtual string getTextToWrite()
    {
        return _text;
    }

    private void writeTextOnMat(OpenCvSharp.Mat mat, string text)
    {
        Cv2.PutText(mat,
                    text,
                    _position,
                    HersheyFonts.HersheySimplex,
                    _fontScale,
                    _color,
                    thickness: _thickness);
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }
}

public class WriteTimeSinkSource : WriteTextSinkSource
{
    public WriteTimeSinkSource(OpenCvSharp.Point position, double fontScale) : base("", position, fontScale)
    {
    }

    protected override string getTextToWrite()
    {
        string label = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff ");

        return label;
    }
}
