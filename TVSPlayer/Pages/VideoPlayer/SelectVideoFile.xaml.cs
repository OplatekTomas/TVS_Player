using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TVS.API;
using NReco.VideoConverter;
using NReco.VideoInfo;
using static System.Environment;
using System.IO;
using System.Drawing;
using System.Windows.Media.Animation;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SelectVideoFile.xaml
    /// </summary>
    public partial class SelectVideoFile : Page {
        public SelectVideoFile(List<Episode.ScannedFile> files) {
            InitializeComponent();
            this.files = files;
        }
        List<Episode.ScannedFile> files;
        Episode.ScannedFile result = null;

        public async Task<Episode.ScannedFile> Show() {
            MainWindow.AddPage(this);
            Episode.ScannedFile result = await Task.Run(async () => {
                while (this.result == null) {
                    await Task.Delay(100);
                }
                return this.result;
            });
            MainWindow.RemovePage();
            if (!String.IsNullOrEmpty(result.NewName)) {
                return result;
            } else {
                return null;
            }
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            Panel.Opacity = 0;
            await Task.Run(() => {
                foreach (var file in files) {
                    if (File.Exists(file.NewName)) {
                        var sample = GetVideo(file);
                        Dispatcher.Invoke(() => {
                            sample.Container.MouseEnter += (s, ev) => { Mouse.OverrideCursor = Cursors.Hand; };
                            sample.Container.MouseLeave += (s, ev) => { Mouse.OverrideCursor = null; };
                            sample.Container.MouseLeftButtonUp += (s, ev) => { result = sample.ScannedFile; };
                            Panel.Children.Add(sample);
                        });
                    }
                }
            });
            var sb = (Storyboard)FindResource("OpacityUp");
            sb.Begin(Panel);
        }

        private VideoFileInfoSample GetVideo(Episode.ScannedFile file) {          
            FFMpegConverter ffmpeg = new FFMpegConverter();
            FFProbe probe = new FFProbe();
            ffmpeg.FFMpegToolPath = probe.ToolPath = Environment.GetFolderPath(SpecialFolder.ApplicationData);
            var info = probe.GetMediaInfo(file.NewName);
            var videotag = info.Streams.Where(x => x.CodecType == "video").FirstOrDefault();
            var audiotag = info.Streams.Where(x => x.CodecType == "audio").FirstOrDefault();
            Stream str = new MemoryStream();
            ffmpeg.GetVideoThumbnail(file.NewName, str, 10);
            Bitmap bmp = new Bitmap(str);
            return Dispatcher.Invoke(() => {
                VideoFileInfoSample sample = sample = new VideoFileInfoSample();
                sample.ScannedFile = file;
                sample.Preview.Source = bmp.ToBitmapImage();
                sample.TopText.Text = videotag.Width + "x" + videotag.Height;
                sample.Codec.Text = videotag.CodecName;
                var lang = videotag.Tags.Where(x => x.Key == "language" || x.Key == "Language" || x.Key == "lang" || x.Key == "Lang").FirstOrDefault();
                sample.Language.Text = !String.IsNullOrEmpty(lang.Value) ? lang.Value : "-";
                sample.Fps.Text = videotag.FrameRate.ToString("##.##") + "FPS";
                sample.Pixel.Text = videotag.PixelFormat;
                sample.Created.Text = File.GetCreationTime(file.NewName).ToString("HH:mm:ss, dd. MM. yyyy");
                sample.AudioCodec.Text = audiotag.CodecName;
                return sample;
            });
        }
    }
}
