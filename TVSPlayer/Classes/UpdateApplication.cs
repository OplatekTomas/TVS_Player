using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TVS.Notification;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace TVSPlayer {
    class UpdateApplication {
        public async static Task CheckForUpdates() {
            await Task.Run(() => {
                try {
                    WebClient wc = new WebClient();
                    wc.Headers.Add("user-agent", "TVS-Player/alpha_dev_build");
                    wc.Headers.Add("Accept", "application/vnd.github.v3+json");
                    var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
                    var jo = JArray.Parse(response).OrderByDescending(x => x["published_at"]).FirstOrDefault();
                    var newDate = DateTime.Parse(jo["published_at"].ToString());
                    if (GetBuildDateTime().AddDays(-1) < newDate) {
                        Settings.UpdateOnStartup = true;
                        NotificationSender sender = new NotificationSender("Update available: \"" + jo["tag_name"] + "\"", "Update will be installed on next application launch. Click here to update now");
                        sender.ClickedEvent += (s, ev) => {
                            Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TVSPlayerUpdater.exe");
                            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                        };
                        sender.Show();
                    }
                } catch (Exception e) {

                }
            });

        }

        public static void StartUpdate() {
            if (Settings.UpdateOnStartup) {
                Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TVSPlayerUpdater.exe");
                Application.Current.Shutdown();
            }
        }


        struct _IMAGE_FILE_HEADER {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        };

        public static DateTime GetBuildDateTime() {
            var path = Assembly.GetExecutingAssembly().Location;
            if (File.Exists(path)) {
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(_IMAGE_FILE_HEADER)), 4)];
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    fileStream.Position = 0x3C;
                    fileStream.Read(buffer, 0, 4);
                    fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fileStream.Read(buffer, 0, 4); // "PE\0\0"
                    fileStream.Read(buffer, 0, buffer.Length);
                }
                var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try {
                    var coffHeader = (_IMAGE_FILE_HEADER)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(_IMAGE_FILE_HEADER));

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond));
                } finally {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }

    }
}
