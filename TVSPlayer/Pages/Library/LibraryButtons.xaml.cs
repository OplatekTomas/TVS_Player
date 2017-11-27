using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Windows.Shapes;
using TVS.API;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for LibraryButtons.xaml
    /// </summary>
    public partial class LibraryButtons : UserControl
    {
        public LibraryButtons(Library library)
        {
            InitializeComponent();
            lib = library;
            sortType = Properties.Settings.Default.LibrarySort;
            viewPosters = Properties.Settings.Default.LibraryView;
        }
        private Library lib;
        public int sortType = 0;
        public bool viewPosters;


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            lib.SetSize(Slider.Value);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Slider.Value = Properties.Settings.Default.LibrarySize;
            Slider.ValueChanged += Slider_ValueChanged;
        }


        private void SortType_MouseUp(object sender, MouseButtonEventArgs e) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s,ev) => {
                SetSort();
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(SortType);
            };
            clone.Begin(SortType);
        }

        public void SetSort(bool newSort=true) {
            Properties.Settings.Default.LibrarySort = sortType;
            Properties.Settings.Default.Save();
            if (newSort) {
                sortType = (sortType == 2) ? 0 : sortType + 1;
            }
            switch (sortType) {
                case 0:
                    lib.SortAlphaPosters(true);
                    lib.SortAlphaList(true);
                    SortImage.SetResourceReference(Image.SourceProperty, "AlphabeticalIcon");
                    break;
                case 1:
                    lib.SortAlphaPosters(false);
                    lib.SortAlphaList(false);
                    SortImage.SetResourceReference(Image.SourceProperty, "AlphabeticalReverseIcon");
                    break;
                case 2:
                    lib.SortCalendarPosters();
                    lib.SortCalendarList();
                    SortImage.SetResourceReference(Image.SourceProperty, "CalendarIcon");
                    break;
            }
        }

        public async Task SetView(bool switchSort = true) {
            Properties.Settings.Default.LibraryView = viewPosters;
            Properties.Settings.Default.Save();
            lib.PanelPosters.Opacity = 0;
            if (viewPosters) {
                ViewImage.SetResourceReference(Image.SourceProperty, "ListIcon");
                await lib.RenderList();
                viewPosters = false;          
            } else {
                ViewImage.SetResourceReference(Image.SourceProperty, "PosterIcon");
                await lib.RenderPosters();
                viewPosters = true;
            }
            SetSort(switchSort);
        }

        private async void AddSeries_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.SelectedPath = Settings.Library + "\\Select Folder";
            if ((bool)fbd.ShowDialog() && fbd.SelectedPath != null) {
                DirectoryInfo di1 = new DirectoryInfo(fbd.SelectedPath);
                DirectoryInfo di2 = new DirectoryInfo(Settings.Library);
                if (di2.FullName == di1.Parent.FullName) {
                    bool isInDb = false;
                    Series series = await MainWindow.SearchShow();
                    if (series.id != 0) { 
                        foreach (Series s in Database.GetSeries()) {
                            if (s.id == series.id) {
                                isInDb = true;
                                break;
                            }
                        }
                        if (!isInDb) {
                            await MainWindow.CreateDatabase(new List<Tuple<int, string>>() { new Tuple<int, string>(series.id, fbd.SelectedPath) });
                            MainWindow.SetPage(new Library());
                        } else {
                            await MessageBox.Show(series.seriesName + " is already in database","Error");
                        }
                    }

                } else {
                   await MessageBox.Show("Selected directory is not in library","Error");
                }
            }
        }

        private void SwitchView_MouseUp(object sender, MouseButtonEventArgs e) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, ev) => {
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(ViewImage);
                 SetView(false);
            };
            clone.Begin(ViewImage);
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                Slider.Value++;
            } else {
                Slider.Value--;
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
