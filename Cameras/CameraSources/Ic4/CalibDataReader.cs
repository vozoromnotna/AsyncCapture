using HDF5CSharp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCapture.Cameras.CameraSources.Ic4;

public record CalibData(Array Pos, List<Mat> Vin, List<Mat> Dist, List<Mat> Mtx, Array Wave, List<Mat> Affine);
public class CalibDataReader
{
    public CalibData ReadData(string fileName)
    {
        if (!Path.Exists(fileName))
        {
            throw new FileNotFoundException(fileName);
        }

        var tree = Hdf5.ReadTreeFileStructure(fileName);
        var fileId = Hdf5.OpenFile(fileName);
        (_, var pos) = Hdf5.ReadDataset<int>(fileId, "/pos");// [8, 4]
        (_, var tr) = Hdf5.ReadDataset<double>(fileId, "/tr");// [8, 2]
        (_, var vin) = Hdf5.ReadDataset<double>(fileId, "/vin");// [8, 1600, 1000]
        (_, var dist) = Hdf5.ReadDataset<double>(fileId, "/dist/dist");// [8, 1, 5]
        (_, var mtx) = Hdf5.ReadDataset<double>(fileId, "/dist/mtx");// [8, 3, 3]
        (_, var wave) = Hdf5.ReadDataset<double>(fileId, "/wave");
        (_, var affine) = Hdf5.ReadDataset<double>(fileId, "/affine"); // [8, 3, 2]

        var vinList =
            readArrayData(vin, (x) => 1.0 / x);
        var distList = readArrayData(dist);
        var mtxList = readArrayData(mtx);
        var affineList = readArrayData(affine);

        return new CalibData(pos, vinList, distList, mtxList, wave, affineList);
    }

    double[] getSubArray(Array array, int index, Func<double, double> func)
    {
        var returnArray = new double[array.GetLength(1) * array.GetLength(2)];

        for (int i = 0; i < array.GetLength(1); i++)
        {
            for (int j = 0; j < array.GetLength(2); j++)
            {
                var val = (double)array.GetValue(index, i, j);
                if (func == null)
                    returnArray[j + i * array.GetLength(2)] = val;
                else
                    returnArray[j + i * array.GetLength(2)] = func(val);
            }
        }

        return returnArray;
    }

    List<Mat> readArrayData(Array data, Func<double, double> func = null)
    {
        var outList = new List<Mat>();
        for (int i = 0; i < data.GetLength(0); i++)
        {
            outList.Add(getSubMat(data, i, func));
        }
        return outList;
    }

    Mat getSubMat(Array array, int index, Func<double, double> func)
    {
        var size = new Size(array.GetLength(2), array.GetLength(1));
        var mat = new Mat(size, MatType.CV_64F);
        var subArray = getSubArray(array, index, func);
        mat.SetArray(subArray);
        return mat;
    }
}
