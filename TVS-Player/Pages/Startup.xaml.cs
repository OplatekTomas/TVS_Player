using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Windows.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Page {
        public Startup() {
            InitializeComponent();
            Api.getToken();
            FadeIn();
        }
        Timer t = new Timer();

        private void OnTimedEvent(object source, ElapsedEventArgs e) {
            double speed = 0.02;       
            Dispatcher.Invoke(new Action(() => {
                WelcomeSign.Opacity += speed;
                if (WelcomeSign.Opacity > 1) {
                    SetupSign.Opacity += speed;
                }
                if (SetupSign.Opacity > 1) {
                    CreateSign.Opacity += speed;
                    ImportShowBlock.Opacity += speed;
                    AddShowBlock.Opacity += speed;
                }
                if (CreateSign.Opacity > 1) {
                    t.Enabled = false;
                }
            }), DispatcherPriority.Send);
        }
        private void FadeIn() {
            WelcomeSign.Opacity = 0;
            SetupSign.Opacity = 0;
            CreateSign.Opacity = 0;
            ImportShowBlock.Opacity = 0;
            AddShowBlock.Opacity = 0;
            t.Interval = 16.7;
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            t.Enabled = true;
        }

        private void ImportShowBlock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Page showPage = new DbLocation("import");
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }

        private void AddShowButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Page showPage = new ShowList("createdb");
            //Page showPage = new ScanLocation();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);

            //Renamer.RenameBatch(new List<int>() { 272644 }, new List<string>() { "D:\\Test\\S1" }, "D:\\Test\\DB");
            //Renamer.RenameBatch(new List<int>(){ 121361 },new List<string>() {"D:\\Test\\S1" }, "D:\\Test\\DB");
            //DatabaseEpisodes.readDb(121361);
            //int k = DatabaseAPI.GetSeasons(121361);
        }
    }
}
