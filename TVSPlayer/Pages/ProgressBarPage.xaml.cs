using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for ProgressBarPage.xaml
    /// </summary>
    public partial class ProgressBarPage : Page {
        public ProgressBarPage(int maximum) {
            InitializeComponent();
            ProgBar.Maximum = maximum;
            Count.Text = "/" + maximum;
            Maximum = maximum;
        }
        public int Maximum { get; set; }
        public ProgressBarPage(int maximum, string text) {
            InitializeComponent();
            ProgBar.Maximum = maximum;
            Count.Text = "/" + maximum;
            MainText.Text = text;
            Maximum = maximum;
        }
        public void SetValue(int value) {
            Count.Text = value+"/"+ Maximum;
            DoubleAnimation animation = new DoubleAnimation(GetValue(value), new TimeSpan(0,0,0,0,200));
            animation.AccelerationRatio = .5;
            animation.DecelerationRatio = .5;
            ProgBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        public void AddValue(int value) {
            SetValue(Convert.ToInt32(ProgBar.Value) + value);
        }


        private double GetValue(int value) {
            if (value > ProgBar.Maximum) return ProgBar.Maximum;
            if (value < 0) return 0;
            return value;
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e) {
            //test();
            Animate();

        }

        private void test() {
            Task.Run(() => {
                double max = 0;
                Dispatcher.Invoke(new Action(() => {
                    max = ProgBar.Maximum;
                }), DispatcherPriority.Send);
                for (int i = 1; i <= max; i++) {
                    Dispatcher.Invoke(new Action(() => {
                        SetValue(i);
                    }), DispatcherPriority.Send);
                    Thread.Sleep(new Random().Next(300, 1500));

                }

            });
        }

        private void Animate() {
            Storyboard sb = (Storyboard)FindResource("Rotate");
            Storyboard temp = sb.Clone();
            temp.Duration = new TimeSpan(0, 0, 0, 1);
            temp.Completed += (s, e) => {
                RotateImage.RenderTransform = new RotateTransform(1);
                Animate(); };
            temp.Begin(RotateImage);
        }

        private void ProgBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (ProgBar.Value == ProgBar.Maximum) {
                MainWindow.RemovePage();
            }
        }
    }
}
