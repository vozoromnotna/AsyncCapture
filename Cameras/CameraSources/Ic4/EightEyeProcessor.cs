using AsyncCapture;
using HDF5CSharp;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraSources.Ic4;

public class EightEyeProcessor : ISink<Mat>
{
    public static int[] GetWavelengths()
    {
        return [700, 750, 800, 650, 600, 550, 500, 450];
    }
    public EightEyeProcessor(CalibData calibData)
    {
        _calibData = calibData;
    }

    private CalibData? _calibData;

    public ISink<Mat> GetSink(int index)
    {
        return sinks[index];
    }

    public void SetSink(ISink<Mat> sink, int index)
    {
        sinks[index] = sink;
    }

    public ISink<Mat>[] sinks = new ISink<Mat>[8];

    int[] _wavelengths = GetWavelengths();

    private Mat[] proccess(Mat input)
    {
        if (_calibData == null)
            throw new Exception("No calib data loaded");

        var vinList = _calibData.Vin;
        var distList = _calibData.Dist;
        var mtxList = _calibData.Mtx;
        var pos = _calibData.Pos;
        var inputSize = input.Size();

        var outArray = new Mat[8]; 

        Parallel.For(0, vinList.Count, (i) =>
        {
            var vinMat = vinList[i];

            var distMat = distList[i];

            var mtxMat = mtxList[i];


            var rect = new OpenCvSharp.Rect((int)pos.GetValue(i, 0), (int)pos.GetValue(i, 1), (int)pos.GetValue(i, 2), (int)pos.GetValue(i, 3));
            outArray[i] = new Mat(input, rect);
            var output = outArray[i];

            if (_vignetting)
            {
                RemoveVignetting(output, output, vinMat);
            }
            
            if (_distortion)
            {
                RemoveDistortion(output, output, distMat, mtxMat);
            }
            

        });
        
        return outArray;
    }

    private bool _vignetting = false;
    public bool VignettingRemove { get => _vignetting; set => _vignetting = value; }

    private bool _distortion = true;

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
            newMeta["index"] = i;
            newMeta["wavelength"] = _wavelengths[i];
            await sinks[i].PutImage(res[i], newMeta);
        });
    }
}


