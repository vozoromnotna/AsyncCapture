using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using DirectShowLib;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Media;

namespace AsyncCapture.Utils
{
    public static class Helper
    {
        public static int FindDeviceByName(string deviceName)
        {
            var systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            return Array.FindIndex(systemCameras, 0, x => x.Name == deviceName);  
        }
        public static string GetStringTime()
        {
            return DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff");
        }
        public static string GetStringTime(DateTime dateTime)
        {
            return dateTime.ToString("dd_MM_yyyy_HH_mm_ss_fff");
        }
        public static string SaveFormatToString(SaveFormat saveFormat)
        {
            switch (saveFormat)
            {
                case SaveFormat.PNG:
                    return ".png";
                case SaveFormat.BMP:
                    return ".bmp";
                case SaveFormat.TIFF:
                    return ".tif";
            }
            return "";
        }

        public static Dictionary<string, string> ExtractPropertiesFromResponse(HttpResponseMessage response)
        {
            // Получаем содержимое ответа
            string responseContent = response.Content.ReadAsStringAsync().Result;

            // Создаем словарь для хранения свойств и значений
            Dictionary<string, string> properties = new Dictionary<string, string>();

            // Регулярное выражение для поиска пар "ключ-значение"
            Regex regex = new Regex(@"(\w+)\s*=\s*([^=\r\n]+)");

            // Ищем все пары "ключ-значение" в строке
            MatchCollection matches = regex.Matches(responseContent);
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                properties[key] = value;
            }

            return properties;
        }

        public static BitmapImage ConvertBitmap(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                return image;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval = null;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                return image;
            }
        }

        public static void ToMat(this BitmapSource source, Mat image)
        {
            if (source.Format == PixelFormats.Bgra32)
            {
                image.Create(source.PixelHeight, source.PixelWidth, MatType.CV_8UC4);
                source.CopyPixels(Int32Rect.Empty, image.Data, (int)image.Step() * image.Rows, (int)image.Step());
            }
            else if (source.Format == PixelFormats.Bgr24)
            {
                image.Create(source.PixelHeight, source.PixelWidth, MatType.CV_8UC3);
                source.CopyPixels(Int32Rect.Empty, image.Data, (int)image.Step() * image.Rows, (int)image.Step());
            }
            else if (source.Format == PixelFormats.Bgr32)
            {
                image.Create(source.PixelHeight, source.PixelWidth, MatType.CV_8UC3);
                source.CopyPixels(Int32Rect.Empty, image.Data, (int)image.Step() * image.Rows, (int)image.Step());
            }
            else
            {
                throw new Exception(String.Format("Conversion from BitmapSource of format {0} is not supported.", source.Format));
            }
        }

        /// <summary>
        /// Convert a BitmapSource into a Mat
        /// </summary>
        /// <param name="source">The Bitmap source</param>
        /// <returns>The resulting Mat</returns>
        public static Mat ToMat(this BitmapSource source)
        {
            Mat result = new Mat();
            source.ToMat(result);
            return result;
        }

        private unsafe delegate void ReadDelegate(byte* ptr, int rows, int cols, double[,] result);
        public static double[,] MatToDouble(Mat image)
        {
            int rows = image.Rows;
            int cols = image.Cols;

            double[,] result = new double[rows, cols];

            unsafe
            {
                ReadDelegate read = null;
                if (image.Type() == MatType.CV_8UC1)
                {
                    read = ReadUint8;
                }
                else if (image.Type() == MatType.CV_16UC1)
                {
                    read = ReadUint16;
                }
                else if (image.Type() == MatType.CV_32FC1)
                {
                    read = ReadFloat;
                }
                else if (image.Type() == MatType.CV_64FC1)
                {
                    read = ReadDouble;
                }
                byte* ptr = (byte*)image.Data.ToPointer();
                read(ptr, rows, cols, result);

            }



            return result;
        }

        unsafe private static void ReadUint16(byte* ptr, int rows, int cols, double[,] result)
        {
            ushort* ushortPtr = (ushort*)ptr;

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    result[i, j] = ushortPtr[index];
                }
            });
        }

        unsafe private static void ReadUint8(byte* ptr, int rows, int cols, double[,] result)
        {
            Parallel.For(0, rows, (i) =>
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    result[i, j] = ptr[index];
                }
            });
        }

        unsafe private static void ReadFloat(byte* ptr, int rows, int cols, double[,] result)
        {
            float* floatPtr = (float*)ptr;

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    result[i, j] = floatPtr[index];
                }
            });
        }

        unsafe private static void ReadDouble(byte* ptr, int rows, int cols, double[,] result)
        {
            double* doublePtr = (double*)ptr;

            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    result[i, j] = doublePtr[index];
                }
            });
        }
    }
}
