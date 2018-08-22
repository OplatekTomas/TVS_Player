using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TVS_Player
{
    class Helper {
        public static SolidColorBrush StringToBrush(string hex) {
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush;
        }

    }
}
