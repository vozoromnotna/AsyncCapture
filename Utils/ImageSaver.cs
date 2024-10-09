
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AsyncCapture.Utils
{
    public enum SaveFormat { [Description(".png")] PNG = 0,[Description(".bmp")] BMP, [Description(".tif")] TIFF, }


    public sealed class ImageSaver
    {
        private string path;

        private ImageSource imageSource;

        private System.Drawing.Bitmap bitmap;

        private Mat mat;

        private string camName;

        private string time;

        SaveFormat saveFormat;

        private Action Save_smth;

        public ImageSaver(ImageSource imageSource, string camName, string path = "", string time = "", SaveFormat saveFormat = SaveFormat.BMP)
        {
            this.imageSource = imageSource;
            this.path = path;
            this.saveFormat = saveFormat;
            this.camName = camName;
            this.time = time;
            Save_smth = SaveImageSource;

        }

        public ImageSaver(System.Drawing.Bitmap bitmap, string camName, string path = "", string time = "", SaveFormat saveFormat = SaveFormat.BMP)
        {
            this.bitmap = bitmap;
            this.path = path;
            this.saveFormat = saveFormat;
            this.camName = camName;
            this.time = time;
            Save_smth = SaveDrawingBMP;
        }

        public ImageSaver(Mat mat, string camName, string path = "", string time = "", SaveFormat saveFormat = SaveFormat.BMP)
        {
            this.mat = mat;
            this.path = path;
            this.saveFormat = saveFormat;
            this.camName = camName;
            this.time = time;

            Directory.CreateDirectory(path);

            Save_smth = SaveMat;
        }



        private string GetBaseName()
        {
            string saveTime;
            saveTime = ((this.time == "")||(this.time == null)) ? DateTime.Now.ToString("d_MM_yyyy_H_m_s_fff") : this.time;

            return $"{path}IMG_{camName}_{saveTime}";
        }
        private void SaveDrawingBMP()
        {
            string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
            switch (saveFormat)
            {
                case SaveFormat.BMP:
                    bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Bmp);
                    break;
                case SaveFormat.PNG:
                    bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case SaveFormat.TIFF:
                    bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Tiff);
                    break;
                default:
                    bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Bmp);
                    break;

            }
            
        }

        private void SaveImageSource()
        {
            string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
            using (var fileStream = new FileStream(save_path, FileMode.Create))
            {
                BitmapEncoder encoder;
                switch (saveFormat)
                {
                    case SaveFormat.PNG:
                        encoder = new PngBitmapEncoder();
                        break;
                    case SaveFormat.BMP:
                        encoder = new BmpBitmapEncoder();
                        break;
                    case SaveFormat.TIFF:
                        encoder = new TiffBitmapEncoder();

                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }
                encoder.Frames.Add(BitmapFrame.Create(imageSource as BitmapSource));
                encoder.Save(fileStream);
            }
        }

        private void SaveMat()
        {
            string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
            mat.SaveImage(save_path);
        }
        public void Save()
        {
            Save_smth();
        }

        public Task SaveAsync()
        {
            return Task.Run(Save_smth);
        }

    }
}
