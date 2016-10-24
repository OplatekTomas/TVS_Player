using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player {
    class DbCreationStruct {
        public struct SelectedShows {
            private string idSel;
            private string pathSel;
            private string nameSel;
            public SelectedShows(string id, string path, string showname) : this() {
                this.idSel = id;
                this.pathSel = path;
                this.nameSel = showname;
            }
            public string getId(){
                return idSel;
             }
            public string getPath() {
                return pathSel;
            }
            public string getName() {
                return nameSel;
            }
        }
        public static List<SelectedShows> ShowsList = new List<SelectedShows>();
    }
}
