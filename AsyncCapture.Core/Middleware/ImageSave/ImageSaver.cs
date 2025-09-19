using System.ComponentModel;
using AsyncCapture.Core.Utils;
using OpenCvSharp;

namespace AsyncCapture.Core.Middleware.ImageSave
{
    public enum SaveFormat { [Description(".png")] PNG = 0,[Description(".bmp")] BMP, [Description(".tif")] TIFF, }


    public sealed class ImageSaver
    {
        private string path;

        // private ImageSource imageSource;

        private System.Drawing.Bitmap bitmap;

        private Mat mat;

        private string camName;

        private string time;

        SaveFormat saveFormat;

        private Func<string> Save_smth;

        // public ImageSaver(ImageSource imageSource, string camName, string path = "", string time = "", SaveFormat saveFormat = SaveFormat.BMP)
        // {
        //     this.imageSource = imageSource;
        //     this.path = path;
        //     this.saveFormat = saveFormat;
        //     this.camName = camName;
        //     this.time = time;
        //     Save_smth = SaveImageSource;
        //
        // }
        //
        // public ImageSaver(System.Drawing.Bitmap bitmap, string camName, string path = "", string time = "", SaveFormat saveFormat = SaveFormat.BMP)
        // {
        //     this.bitmap = bitmap;
        //     this.path = path;
        //     this.saveFormat = saveFormat;
        //     this.camName = camName;
        //     this.time = time;
        //     Save_smth = SaveDrawingBMP;
        // }

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
            var saveTime = (String.IsNullOrEmpty(time) ? DateTime.Now.ToString("d_MM_yyyy_H_m_s_fff") : time);

            var filename = $"IMG_{camName}_{saveTime}";
            return Path.Combine(path, filename);
        }
        // private string SaveDrawingBMP()
        // {
        //     string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
        //     switch (saveFormat)
        //     {
        //         case SaveFormat.BMP:
        //             bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Bmp);
        //             break;
        //         case SaveFormat.PNG:
        //             bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Png);
        //             break;
        //         case SaveFormat.TIFF:
        //             bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Tiff);
        //             break;
        //         default:
        //             bitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Bmp);
        //             break;
        //
        //     }
        //     return save_path;
        //     
        // }

        // private string SaveImageSource()
        // {
        //     string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
        //     using (var fileStream = new FileStream(save_path, FileMode.Create))
        //     {
        //         BitmapEncoder encoder;
        //         switch (saveFormat)
        //         {
        //             case SaveFormat.PNG:
        //                 encoder = new PngBitmapEncoder();
        //                 break;
        //             case SaveFormat.BMP:
        //                 encoder = new BmpBitmapEncoder();
        //                 break;
        //             case SaveFormat.TIFF:
        //                 encoder = new TiffBitmapEncoder();
        //
        //                 break;
        //             default:
        //                 encoder = new PngBitmapEncoder();
        //                 break;
        //         }
        //         encoder.Frames.Add(BitmapFrame.Create(imageSource as BitmapSource));
        //         encoder.Save(fileStream);
        //     }
        //     return save_path;
        // }

        private string SaveMat()
        {
            string save_path = GetBaseName() + Helper.SaveFormatToString(saveFormat);
            
            if (!(mat.Type() == MatType.CV_8UC1 || mat.Type() != MatType.CV_8UC3))
            {
                Cv2.Normalize(mat, mat, 0, 255, NormTypes.MinMax);
            }

            mat.SaveImage(save_path);
            return save_path;
        }
        public string Save()
        {
            return Save_smth();
        }

        public Task<string> SaveAsync()
        {
            return Task.Run(Save_smth);
        }

    }
}
