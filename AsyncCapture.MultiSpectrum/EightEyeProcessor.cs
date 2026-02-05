using AsyncCapture.Core;
using OpenCvSharp;
using System;

namespace AsyncCapture.MultiSpectrum;

public class EightEyeProcessor : ISink<Mat>
{
    public static int[] GetWavelengths()
    {
        return [450, 500, 550, 600, 650, 700, 750, 800];
    }
    public int[] GetOrderedWavelengths()
    {
        var wavelengths = new int[_calibData.Wave.Length];
        for (int i = 0; i < _calibData.Wave.Length; i++)
        {
            wavelengths[i] = (int)(double)_calibData.Wave.GetValue(i, 0);
        }
        return wavelengths;
        
    }

    private int[] _wavelengths;
    private OpenCvSharp.Rect[] _rects;
    private readonly object _calibLock = new();
    public EightEyeProcessor(CalibData calibData)
    {
        _calibData = calibData;
        PreReadCalib();
        _wavelengths = GetOrderedWavelengths();
    }

    private void PreReadCalib()
    {
        var pos = _calibData.Pos;
        _rects = new Rect[pos.GetLength(0)];
        for (int i = 0; i < _rects.Length; i++)
        {
            _rects[i] = new OpenCvSharp.Rect((int)pos.GetValue(i, 0), (int)pos.GetValue(i, 1), (int)pos.GetValue(i, 2), (int)pos.GetValue(i, 3));
        }
    }

    private CalibData? _calibData;

    public void SetCalibData(CalibData calibData)
    {
        if (calibData == null)
            throw new ArgumentNullException(nameof(calibData));

        lock (_calibLock)
        {
            _calibData = calibData;
            PreReadCalib();
            _wavelengths = GetOrderedWavelengths();
        }
    }

    public ISink<Mat> GetSink(int index)
    {
        return sinks[index];
    }

    public void SetSink(ISink<Mat> sink, int index)
    {
        sinks[index] = sink;
    }

    public ISink<Mat>[] sinks = new ISink<Mat>[8];

    private Mat[] proccess(Mat input)
    {
        CalibData calibData;
        OpenCvSharp.Rect[] rects;
        lock (_calibLock)
        {
            if (_calibData == null)
                throw new Exception("No calib data loaded");
            calibData = _calibData;
            rects = _rects;
        }

        var vinList = calibData.Vin;
        var distList = calibData.Dist;
        var mtxList = calibData.Mtx;
        var pos = calibData.Pos;
        var inputSize = input.Size();
        var affineList = calibData.Affine;
        var outArray = new Mat[8]; 

        //for (int i = 0; i < vinList.Count; i++)
        //{
            Parallel.For(0, vinList.Count, (i) =>
            {
                var vinMat = vinList[i];

                var distMat = distList[i];

                var mtxMat = mtxList[i];


                var rect = rects[i];


                outArray[i] = new Mat(input, rect);

                
                var output = outArray[i];

                Cv2.Resize(output, output, rects[0].Size, interpolation: InterpolationFlags.Cubic);

                if (_vignetting)
                {
                    RemoveVignetting(output, output, vinMat);
                }

                if (_distortion)
                {
                    RemoveDistortion(output, output, distMat, mtxMat);
                }

                //if (affineList.Count() > 0)
                //{
                //    var affineMat = affineList[i];
                //    var aff = Helper.MatToDouble(affineMat);
                //    Cv2.WarpAffine(output, output, affineMat.T(), output.Size());
                //}

            });

        return outArray;
    }

    private bool _vignetting = false;
    public bool VignettingRemove { get => _vignetting; set => _vignetting = value; }

    private bool _distortion = false;

    public bool DistortionRemove { get => _distortion; set => _distortion = value; }

    private void RemoveDistortion(Mat input, Mat output, Mat distMat, Mat mtxMat)
    {
        var outUdistort = new Mat();
        Cv2.Undistort(output, outUdistort, mtxMat, distMat);
        Cv2.CopyTo(outUdistort, output);
    }

    private void RemoveVignetting(Mat input, Mat output, Mat vinMat)
    {
        Cv2.Multiply(input, vinMat, output, dtype: (int)output.Type());
    }
    public async Task PutImage(Mat image, Dictionary<string, object> meta)
    {
        var res = proccess(image);
        
        await Parallel.ForAsync(0, 8, async (i, token) =>
        {
            var newMeta = new Dictionary<string, object>();
            CopyDict(meta, newMeta);
            newMeta["index"] = i;
            newMeta["wavelength"] = _wavelengths[i];
            await sinks[i].PutImage(res[i], newMeta);
        });
    }

    private void CopyDict(Dictionary<string, object> oldDict, Dictionary<string, object> newDict)
    {
        foreach (var key in oldDict.Keys)
        {
            newDict[key] = oldDict[key];
        }
    }
}


