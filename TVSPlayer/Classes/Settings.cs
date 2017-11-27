using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TVSPlayer {
    static class Settings {
        private static string library;
        private static bool autodownload;
        private static string cachelocation;
        private static string scanone;
        private static string scantwo;
        private static string scanthree;
        private static bool theme;
        private static DateTime lastCheck;
        private static int downloadSpeed;
        private static int uploadSpeed;
        private static bool stremedBefore;
        private static TorrentQuality downloadquality;
        private static TorrentQuality streamquality;
        private static bool useWinDefaultPlayer;

        public static string Library { get { return library; } set { library = value; SaveSettings(); } }
        public static bool AutoDownload { get { return autodownload; } set { autodownload = value; SaveSettings(); } }
        public static string DownloadCacheLocation { get { return cachelocation; } set { cachelocation = value; SaveSettings(); } }
        public static string FirstScanLocation { get { return scanone; } set { scanone = value; SaveSettings(); } }
        public static string SecondScanLocation { get { return scantwo; } set { scantwo = value; SaveSettings(); } }
        public static string ThirdScanLocation { get { return scanthree; } set { scanthree = value; SaveSettings(); } }
        public static bool Theme  { get { return theme; } set { theme = value; SaveSettings(); } }
        public static DateTime LastCheck { get { return lastCheck; } set { lastCheck = value; SaveSettings(); } }
        public static TorrentQuality DownloadQuality { get { return downloadquality; } set { downloadquality = value; SaveSettings(); } }
        public static TorrentQuality StreamQuality { get { return streamquality; } set { streamquality = value; SaveSettings(); } }
        public static int UploadSpeed { get { return uploadSpeed; } set { uploadSpeed = value; SaveSettings(); } }
        public static int DownloadSpeed { get { return downloadSpeed; } set { downloadSpeed = value; SaveSettings(); } }
        public static bool StreamedBefore { get { return stremedBefore; } set { stremedBefore = value; SaveSettings(); } }
        public static bool UseWinDefaultPlayer { get { return useWinDefaultPlayer; } set { useWinDefaultPlayer = value; SaveSettings(); } }



        /// <summary>
        /// Saves Settings. Is called automatically whenever property value is changed
        /// </summary>
        public static void SaveSettings() {
            Type type = typeof(Settings);
            string filename = Helper.data + "Settings.tvsp";
            // Library = "test";
            // Test = Library;
            CreateDir();
            if (!File.Exists(filename)) {
                File.Create(filename).Dispose();
            }
            do {
                try {
                    FieldInfo[] properties = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
                    object[,] a = new object[properties.Length, 2];
                    int i = 0;
                    foreach (FieldInfo field in properties) {
                        a[i, 0] = field.Name;
                        a[i, 1] = field.GetValue(null);
                        i++;
                    };
                    string json = JsonConvert.SerializeObject(a);
                    StreamWriter sw = new StreamWriter(filename);
                    sw.Write(json);
                    sw.Close();
                    return;
                } catch (IOException e) {
                    Thread.Sleep(10);
                }
            } while (true);
        }

        private static void CreateDir() {
            string filename = Helper.data + "Settings.tvsp";
            if (!Directory.Exists(Path.GetDirectoryName(filename))){
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
        }
        public static object GetDefault(Type type) {
            if (type.IsValueType) {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Loads settings with default value if new settings has been added. In case of enums edit code - might get "crashy" if you dont
        /// </summary>
        public static void Load() {
            Type type = typeof(Settings);
            string filename = Helper.data + "Settings.tvsp";
            CreateDir();
            if (!File.Exists(filename)) {
                File.Create(filename).Dispose();
            }
            do {
                try {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
                    object[,] a;
                    StreamReader sr = new StreamReader(filename);
                    string json = sr.ReadToEnd();
                    sr.Close();
                    if (!String.IsNullOrEmpty(json)) {
                        JArray ja = JArray.Parse(json);
                        a = ja.ToObject<object[,]>();
                        if (a.GetLength(0) != fields.Length) { }
                        int i = 0;
                        foreach (FieldInfo field in fields) {
                            try {
                                if (field.Name == (a[i, 0] as string)) {
                                    try {
                                        field.SetValue(null, Convert.ChangeType(a[i, 1], field.FieldType));
                                    } catch (InvalidCastException e) {
                                        field.SetValue(null, (TorrentQuality)Enum.ToObject(typeof(TorrentQuality), a[i, 1]));
                                    }
                                }
                            } catch (IndexOutOfRangeException) {
                                field.SetValue(null, GetDefault(field.FieldType));
                            }
                            i++;
                        };
                    }
                    return;
                } catch (IOException e) {
                    Thread.Sleep(15);
                }
            } while (true);
        }
    }
}
