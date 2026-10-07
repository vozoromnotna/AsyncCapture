using HDF5CSharp;
using OpenCvSharp;

namespace AsyncCapture.MultiSpectrum;

public record CalibData(Array Pos, List<Mat> Vin, List<Mat> Dist, List<Mat> Mtx, Array Wave, List<Mat> Affine);
public class CalibDataReader
{
    public CalibData ReadData(string fileName)
    {
        if (!Path.Exists(fileName))
        {
            throw new FileNotFoundException(fileName);
        }

        var fileId = Hdf5.OpenFile(fileName);
        List<Mat>? vinList = null, distList = null, mtxList = null, affineList = null;
        try
        {
            (_, var pos) = Hdf5.ReadDataset<int>(fileId, "/pos");// [8, 4]
            (_, var tr) = Hdf5.ReadDataset<double>(fileId, "/tr");// [8, 2], read for legacy profile compatibility
            (_, var vin) = Hdf5.ReadDataset<double>(fileId, "/vin");// [8, H, W]
            (_, var dist) = Hdf5.ReadDataset<double>(fileId, "/dist/dist");// [8, 1, 5]
            (_, var mtx) = Hdf5.ReadDataset<double>(fileId, "/dist/mtx");// [8, 3, 3]
            (_, var wave) = Hdf5.ReadDataset<double>(fileId, "/wave");
            var hasAffine = Hdf5Utils.ItemExists(
                fileId,
                "/affine",
                HDF5CSharp.DataTypes.Hdf5ElementType.Dataset);

            vinList = readArrayData(vin, x => 1.0 / x);
            distList = readArrayData(dist);
            mtxList = readArrayData(mtx);
            if (hasAffine)
            {
                (_, var affine) = Hdf5.ReadDataset<double>(fileId, "/affine"); // [8, 3, 2]
                affineList = readArrayData(affine);
            }
            else
            {
                // Legacy P0 profiles omit /affine. EightEye currently does not apply
                // this dataset; represent that absence as an identity transform.
                affineList = CreateIdentityAffines();
            }
            return new CalibData(pos, vinList, distList, mtxList, wave, affineList);
        }
        catch
        {
            Dispose(vinList); Dispose(distList); Dispose(mtxList); Dispose(affineList);
            throw;
        }
        finally { Hdf5.CloseFile(fileId); }
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
        try
        {
            for (int i = 0; i < data.GetLength(0); i++)
                outList.Add(getSubMat(data, i, func));
            return outList;
        }
        catch
        {
            Dispose(outList);
            throw;
        }
    }

    Mat getSubMat(Array array, int index, Func<double, double> func)
    {
        var size = new Size(array.GetLength(2), array.GetLength(1));
        var mat = new Mat(size, MatType.CV_64F);
        try
        {
            var subArray = getSubArray(array, index, func);
            mat.SetArray(subArray);
            return mat;
        }
        catch
        {
            mat.Dispose();
            throw;
        }
    }

    private static void Dispose(List<Mat>? mats)
    {
        if (mats == null) return;
        foreach (var mat in mats) mat.Dispose();
    }

    private static List<Mat> CreateIdentityAffines()
    {
        var result = new List<Mat>(8);
        try
        {
            for (var index = 0; index < 8; index++)
            {
                var affine = new Mat(3, 2, MatType.CV_64FC1);
                try
                {
                    affine.SetArray(new double[] { 1, 0, 0, 1, 0, 0 });
                    result.Add(affine);
                }
                catch
                {
                    affine.Dispose();
                    throw;
                }
            }
            return result;
        }
        catch
        {
            Dispose(result);
            throw;
        }
    }
}
