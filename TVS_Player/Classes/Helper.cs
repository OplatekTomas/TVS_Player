using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TVS_Player_Base;

namespace TVS_Player
{
    class Helper {
        public static SolidColorBrush StringToBrush(string hex) {
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush;
        }

        public static async Task<BitmapImage> GetImage(string url) {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = GetImageUrl(url);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            while (bitmap.IsDownloading) {
                await Task.Delay(5);
            }
            bitmap.Freeze();
            return bitmap;
        }

        public static Uri GetImageUrl(string url) {
            return new Uri("http://" + Api.Ip + ":" + Api.Port + url);
        }

    }
    public static class Extensions {
        public static void Save(this BitmapImage image, string filePath) {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create)) {
                encoder.Save(fileStream);
            }
        }
    }
}
