using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer {
    class Helper {
        /// <summary>
        /// Path to cached data
        /// </summary>
        public static string data = @"C:\Users\Public\Documents\TVS-Player\";

        /// <summary>
        /// Link for retrieving poster data
        /// </summary>
        public static string posterLink = "https://www.thetvdb.com/banners/";

        /// <summary>
        /// Generates default name for episode. SeriesName - SxxExx - EpisodeName
        /// </summary>
        /// <param name="episode">Which episode to generate for</param>
        /// <param name="series">Which season to generate for</param>
        public static string GenerateName(Series series, Episode episode) {
            string name = null;
            if (episode.airedSeason < 10) {
                name = episode.airedEpisodeNumber < 10 ? series.seriesName + " - S0" + episode.airedSeason + "E0" + episode.airedEpisodeNumber + " - " + episode.episodeName : name = series.seriesName + " - S0" + episode.airedSeason + "E" + episode.airedEpisodeNumber + " - " + episode.episodeName;
            } else if (episode.airedSeason >= 10) {
                name = episode.airedEpisodeNumber < 10 ? series.seriesName + " - S" + episode.airedSeason + "E0" + episode.airedEpisodeNumber + " - " + episode.episodeName : series.seriesName + " - S" + episode.airedSeason + "E" + episode.airedEpisodeNumber + " - " + episode.episodeName;
            }
            return name;
        }

        /// <summary>
        /// Returns SxxExx according to Episode
        /// </summary>
        /// <param name="episode">Any Episode</param>
        /// <returns></returns>
        public static string GenerateName(Episode episode) {
            if (episode.airedSeason < 10) {
                return episode.airedEpisodeNumber < 10 ? "S0" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S0" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            } else {
                return episode.airedEpisodeNumber < 10 ? "S" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            }
        }

        /// <summary>
        /// Get string in format h:mm:ss from player media lenght or current position
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetTime(long value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value / 60 - 60 * hours)));

            seconds = Convert.ToInt32(Math.Floor((double)(value - ((60 * 60 * hours) + (60 * minutes)))));
            string hoursString = hours > 0 ? hours + ":" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }


        /// <summary>
        /// Cheks if TVSPlyer is already running
        /// </summary>
        /// <returns></returns>
        public static bool CheckRunning() {
            var procName = Process.GetCurrentProcess();
            List<Process> processes = Process.GetProcessesByName(procName.ProcessName).ToList();
            processes.Remove(procName);
            if (processes.Count > 1) {
                foreach (var proc in processes) {
                    BringProcessToFront(proc);
                }
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Brings any process to front
        /// </summary>
        /// <param name="process"></param>
        public static void BringProcessToFront(Process process) {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle)) {
                ShowWindow(handle, SW_RESTORE);
            }
            SetForegroundWindow(handle);
        }
        /// <summary>
        /// Disables Windows screen saver
        /// </summary>
        public static void DisableScreenSaver() {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }
        /// <summary>
        /// Enables Windows screen saver
        /// </summary>
        public static void EnableScreenSaver() {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }


        [DllImport("kernel32.dll")]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        enum EXECUTION_STATE : uint {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

    }

    static class Extensions{

        public static Color ToMediaColor(this System.Drawing.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Waits for all Tasks in IEnumerable to complete
        /// </summary>
        /// <param name="tasks"></param>
        public static void WaitAll(this IEnumerable<Task> tasks) {
            Task.WaitAll(tasks.ToArray());
        }
    }
}
