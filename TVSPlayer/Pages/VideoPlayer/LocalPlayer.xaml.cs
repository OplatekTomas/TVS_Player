using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using TVS.API;
using static System.Environment;
using static TVS.API.Episode;
using NReco.VideoInfo;
using NReco.VideoConverter;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for LocalPlayer.xaml
    /// </summary>
    public partial class LocalPlayer : Page {
        public LocalPlayer(Series series, Episode episode) {
            InitializeComponent();
            this.series = series;
            this.episode = Database.GetEpisode(series.id, episode.id);
        }

        public Series series;
        public Episode episode;
        ScannedFile scannedFile;
        string fileLenght;
        DispatcherTimer positionUpdate = new DispatcherTimer();
        List<FoundSubtitle> allSubtitles = new List<FoundSubtitle>();
        List<SubtitleItem> subtitles = new List<SubtitleItem>();

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            scannedFile = await GetFile(episode);
            LoadSidebar();
            Helper.DisableScreenSaver();
            MainWindow.HideContent();
            MainWindow.videoPlayback = true;
            PlayerPage.Focus();
            VolumeSlider.Value = Player.Volume = Properties.Settings.Default.Volume;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            Player.MediaFailed += (s, ev) => MediaFailedEvent();
            Player.MediaEnded += (s, ev) => MediaFinishedEvent();
            Player.MediaOpened += (s, ev) => MediaOpenedEvent();
            Player.Source = new Uri(scannedFile.NewName);
        }

        private async Task<ScannedFile> GetFile(Episode episode) {
            var files = episode.files.Where(x => x.Type == ScannedFile.FileType.Video).ToList();
            if (files.Count == 1) {
                return files[0];
            }
            SelectVideoFile select = new SelectVideoFile(files);
            var result = await select.Show();
            if (result != null) {
                return result;
            }
            return null;
        }

        private void MediaFailedEvent() {
            Dispatcher.Invoke(async () => {
                await MessageBox.Show("Playback failed.", "Error");
                Return();
            }, DispatcherPriority.Send);
        }

        private void MediaFinishedEvent() {
            Return();
        }


        private async Task LoadSidebar() {
            List<SubtitleItem> subs = new List<SubtitleItem>();
            CurrentStatus.Text = "Loading info";
            await Task.Run(() => {
                FFProbe probe = new FFProbe();
                probe.ToolPath = Environment.GetFolderPath(SpecialFolder.ApplicationData);
                var info = probe.GetMediaInfo(scannedFile.NewName);
                var video = info.Streams.Where(x => x.CodecType == "video").FirstOrDefault();
                var audio = info.Streams.Where(x => x.CodecType == "audio").FirstOrDefault();
                Dispatcher.Invoke(() => {
                    EpisodeName.Text = episode.episodeName;
                    SeriesNumber.Text = Helper.GenerateName(episode);
                    Framerate.Text = "Framerate: " + video.FrameRate.ToString("##.##");
                    Resolution.Text = "Resolution: " + video.Width + "x" + video.Height;
                    VideoCodec.Text = "Video codec: " + video.CodecLongName;
                    PixelFormat.Text = "Pixel format: " + video.PixelFormat;
                    AudioCodec.Text = "Audio codec: " + audio.CodecLongName;
                });
                var multiple = info.Streams.Where(x => x.CodecType == "subtitle").Where(x => x.CodecName == "srt");
                var single = multiple.FirstOrDefault();
                if (single != null) {
                    FFMpegConverter converter = new FFMpegConverter();
                    converter.FFMpegToolPath = probe.ToolPath;
                    Stream str = new MemoryStream();
                    converter.ConvertMedia(scannedFile.NewName, str, "srt");
                    FoundSubtitle fs = new FoundSubtitle(str);
                    fs.Version = "Packed with video";
                    allSubtitles.Add(fs);
                } else {
                    var subtitles = episode.files.Where(x => x.Type == ScannedFile.FileType.Subtitles);
                    foreach (var sub in subtitles) {
                        FoundSubtitle fs = new FoundSubtitle(sub.NewName);
                        fs.Version = sub.OriginalName;
                        allSubtitles.Add(fs);
                    }
                }
                LoadSideBarSubs();
                
            });
            CurrentStatus.Text = "";
            RenderSubs();
        }

        private void LoadSideBarSubs() {
            foreach (var item in allSubtitles) {
                Dispatcher.Invoke(() => {
                    SubtitlePicker picker = new SubtitlePicker(item);
                    picker.Version.Text =  Path.GetFileNameWithoutExtension(item.Version);
                    picker.MouseLeftButtonUp += (s, ev) => SelectSubtitiles(picker);
                    SubtitleSelectionPanel.Children.Add(picker);
                });

            }
            Dispatcher.Invoke(() => {
                SubtitlePicker toLoad = null;
                foreach (SubtitlePicker pick in SubtitleSelectionPanel.Children) {
                    if (pick.Subtitle.Stream != null) {
                        toLoad = pick;
                    }
                }
                if (toLoad == null && SubtitleSelectionPanel.Children.Count > 0) {
                    toLoad = (SubtitlePicker)SubtitleSelectionPanel.Children[0];
                }
                if (toLoad != null) { 
                    SelectSubtitiles(toLoad);
                }
            });         
        }

        private void SelectSubtitiles(SubtitlePicker picker) {
            if (picker.Subtitle.File != null) {
                var lines = Subtitles.ParseSubtitleItems(picker.Subtitle.File);
                if (lines.Count > 1) {
                    subtitles = lines;
                }
            } else if (picker.Subtitle.Stream != null) {
                picker.Subtitle.Stream.Position = 0;
                var lines = Subtitles.ParseSubtitleItems(new StreamReader(picker.Subtitle.Stream).ReadToEnd(), ".srt");
                if (lines.Count > 1) {
                    subtitles = lines;
                }
            }
            NoSubtitles.ItemSelected.Opacity = 0;
            int index = SubtitleSelectionPanel.Children.IndexOf(picker);
            picker.ItemSelected.Opacity = 1;
            foreach (SubtitlePicker pick in SubtitleSelectionPanel.Children) {
                if (SubtitleSelectionPanel.Children.IndexOf(pick) != index) {
                    pick.ItemSelected.Opacity = 0;
                }
            }
        }

        private void RenderSubs() {
            int index = 0;
            long position = 0;
            bool isLoaded = IsLoaded;
            Task.Run(async () => {
                await Task.Delay(500);
                while (isLoaded) {
                    var subtitles = this.subtitles;
                    if (subtitles.Count > 1) {
                        Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                        while (!(subtitles[index].StartTime < position && subtitles[index + 1].StartTime > position)) {
                            Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                            if (subtitles[index].StartTime < position) {
                                index++;
                            } else if (subtitles[index].StartTime > position) {
                                index--;
                            }
                        }
                        if (subtitles[index].EndTime >= position) {
                            Dispatcher.Invoke(() => {
                                foreach (var line in subtitles[index].Lines) {
                                    if (!SubtitlePanel.Children.Contains(line)) {
                                        foreach (TextBlock childElement in line.Children) {
                                            childElement.FontSize = Settings.SubtitleSize > 20 ? Settings.SubtitleSize : 36;
                                        }
                                        SubtitlePanel.Children.Add(line);
                                    }
                                }
                            }, DispatcherPriority.Send);
                        }
                        while (subtitles[index].EndTime >= position) {
                            Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                            await Task.Delay(5);
                            if (subtitles[index].StartTime > position) {
                                break;
                            }
                        }
                        Dispatcher.Invoke(() => {
                            SubtitlePanel.Children.Clear();
                        }, DispatcherPriority.Send);
                        await Task.Delay(1);
                    } else {
                        index = 0;
                        await Task.Delay(500);
                    }
                    Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                }
            });
        }

        private long GetMiliseconds(long value) {
            return value / 10000;
        }

        private void MediaOpenedEvent() {
            VideoSlider.Maximum = Player.MediaDuration;
            if (!episode.finished) { Player.MediaPosition = episode.continueAt; }
            episode.finished = false;
            fileLenght = GetTime(Player.MediaDuration);
            positionUpdate.Interval = new TimeSpan(0, 0, 1);
            positionUpdate.Tick += new EventHandler(UpdatePosition);
            positionUpdate.Start();
            Player.Play();
        }

        private string GetTime(long value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value / 60 - 60 * hours)));

            seconds = Convert.ToInt32(Math.Floor((double)(value - ((60 * 60 * hours) + (60 * minutes)) )));
            string hoursString = hours > 0 ? hours + ":" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }


        private void UpdatePosition(object sender, EventArgs e) {
            VideoSlider.Value = Player.MediaPosition;
            CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + fileLenght;
        } 

        DispatcherTimer contentUpdate = new DispatcherTimer();
        private void VideoSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            contentUpdate.Stop();
            contentUpdate = new DispatcherTimer();
            contentUpdate.Interval = new TimeSpan(0, 0, 0, 0, 500);
            contentUpdate.Tick += (s, ev) => {
                Player.MediaPosition = Convert.ToInt64(VideoSlider.Value);
                CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + fileLenght;
            };
            contentUpdate.Start();
            Player.Pause();
            positionUpdate.Stop();
        }

        private void VideoSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            Player.MediaPosition = Convert.ToInt64(VideoSlider.Value);
            contentUpdate.Stop();
            if (isPlaying) { 
                Player.Play();
            }
            positionUpdate.Start();
        }

        DispatcherTimer timer;
        bool isRunning = false;
        private void Page_MouseMove(object sender, MouseEventArgs e) {
            if (!isRunning) {
                isRunning = true;
                var sb = (Storyboard)FindResource("OpacityUp");
                var clone = sb.Clone();
                Mouse.OverrideCursor = null;
                clone.Completed += (s, ev) => {
                    isRunning = false;

                };
                clone.Begin(TopBar);
                clone.Begin(BottomBar);
                if (timer != null) {
                    timer.Tick -= Timer_Tick;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 1, 500);
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var sb = (Storyboard)FindResource("OpacityDown");
            Mouse.OverrideCursor = Cursors.None;
            sb.Begin(TopBar);
            sb.Begin(BottomBar);
        }

        private void PlayIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Pause();
        }

        private void Page_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
            }
        }


        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Properties.Settings.Default.Volume = Player.Volume = VolumeSlider.Value;
            Task.Run(() => {
                Properties.Settings.Default.Save();
            });
            if (VolumeSlider.Value == VolumeSlider.Minimum) {
                Player.Volume = 0;
            }
        }

        private void FullscreenIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
        }

        private void AlwaysTopIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SwitchState(MainWindow.PlayerState.PiP);
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Return();
        }

        //Yes I do know this can be simplified but that would result in slower pause and I do not want that
        bool isPlaying = true;
        private void Pause() {
            if (isPlaying) {
                Player.Pause();
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-play-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
            } else {
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-pause-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
                Player.Play();
            }
            isPlaying = !isPlaying;
        }

        double lastValue = 0.85;
        private void Mute() {
            if (Player.Volume == 0) {
                Player.Volume = lastValue;
            } else {
                lastValue = Player.Volume;
                Player.Volume = 0;
            }
        }

        private void GoForward() {
            Player.MediaPosition += 100000000;
        }

        private void GoBack() {
            Player.MediaPosition -= 100000000;
        }

        private async void Return() {
            Helper.EnableScreenSaver();
            MainWindow.ShowContent();
            PlayerPage.MouseMove -= Page_MouseMove;
            timer.Stop();
            MainWindow.videoPlayback = false;
            MainWindow.SwitchState(MainWindow.PlayerState.Normal);
            episode.continueAt = Player.MediaPosition - 50000000 > 0 ? Player.MediaPosition - 50000000 : 0;
            episode.finished = Player.MediaDuration - 3000000000 < Player.MediaPosition ? true : false;
            Database.EditEpisode(series.id, episode.id, episode);
            MainWindow.RemovePage();
            SeriesEpisodes.TryRefresh();
            Mouse.OverrideCursor = null;
        }

        private void PlayerPage_PreviewKeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Space:
                    Pause();
                    break;
                case Key.Escape:
                    Return();
                    break;
                case Key.F:
                    MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
                    break;
                case Key.M:
                    Mute();
                    break;
                case Key.Right:
                    GoForward();
                    break;
                case Key.Left:
                    GoBack();
                    break;
            }
            e.Handled = true;
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void VideoSlider_GotFocus(object sender, RoutedEventArgs e) {
            Focus();
        }

        private void SubtitlesIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            RightPanel.Visibility = Hider.Visibility = Visibility.Visible;
            var sb = ((Storyboard)FindResource("OpacityUp"));
            sb.Begin(RightPanel);
            ThicknessAnimation thicc = new ThicknessAnimation(new Thickness(0), new TimeSpan(0, 0, 0, 0, 300));
            RightPanel.BeginAnimation(MarginProperty, thicc);
        }

        private void HideSideBar_MouseUp(object sender, MouseButtonEventArgs e) {
            var sb = ((Storyboard)FindResource("OpacityDown")).Clone();
            sb.Completed += (s, ev) => {
                RightPanel.Visibility = Hider.Visibility = Visibility.Hidden;
            };
            sb.Begin(RightPanel);
            ThicknessAnimation thicc = new ThicknessAnimation(new Thickness(0,0,-250,0), new TimeSpan(0, 0, 0, 0, 300));
            RightPanel.BeginAnimation(MarginProperty, thicc);
        }

        private void SubtitleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            Settings.SubtitleSize = (int)SubtitleSlider.Value;
        }

        public class FoundSubtitle {
            public FoundSubtitle(Stream stream) {
                Stream = stream;
            }
            public FoundSubtitle(string file) {
                File = file;
            }
            public string File { get; set; }
            public Stream Stream { get; set; }
            public string Version { get; set; }
        }

        private void NoSubtitles_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            NoSubtitles.ItemSelected.Opacity = 1;           
            foreach (SubtitlePicker pick in SubtitleSelectionPanel.Children) {
                pick.ItemSelected.Opacity = 0;
            }
            subtitles = new List<SubtitleItem>();
        }
    }
}
