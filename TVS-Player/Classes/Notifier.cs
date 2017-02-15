using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TVS_Player {
    class Notifier {
        Notification n;
        List<Show> sh;
        public void UpdateDBAll() {
            n = new Notification();
            sh = DatabaseShows.ReadDb();
            n.ProgBar.Maximum = sh.Count;
            MainWindow.notifications.Add(n);
            foreach (Show s in sh) {
                int sIndex = sh.IndexOf(s)+1;
                n.MainText.Text = "Updating: " + s.name;
                n.SecondText.Text = sIndex + "/" + sh.Count;
                n.ProgBar.Value = sIndex;
                int index = MainWindow.notifications.IndexOf(n);
                MainWindow.notifications[index] = n;
                //Checker.UpdateFull(s.id);
            }
            MainWindow.notifications.Remove(n);

        }

    }
}
