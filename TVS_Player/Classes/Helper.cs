using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            bitmap.UriSource = new Uri("http://" + Api.Ip + ":" + Api.Port + url);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            while (bitmap.IsDownloading) {
                await Task.Delay(5);
            }
            bitmap.Freeze();
            return bitmap;
        }

        public static RenderTargetBitmap RenderElement(FrameworkElement element) {
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            double dpi = 96;
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            dc.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, width, height));
            dc.Close();
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);
            bitmap.Render(visual);
            return bitmap;
        }

        public static bool ParseDate(string date, out DateTime result) {
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime success)) {
                result = success;
                return true;
            } else {
                result = DateTime.MinValue;
                return false;
            }
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
