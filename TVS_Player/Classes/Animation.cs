using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TVS_Player{
    class Animate {
        public static void FadeIn(FrameworkElement control, Action continueWith = null) {
            StartAnimation(FindResource("FadeIn"), control, continueWith);
        }

        public static void FadeOut(FrameworkElement control, Action continueWith = null) {
            StartAnimation(FindResource("FadeOut"), control, continueWith);
        }

        private static void StartAnimation(Storyboard sb, FrameworkElement control, Action continueWith = null) {
            if (continueWith != null) {
                Storyboard temp = sb.Clone();
                temp.Completed += (s, ev) => continueWith();
                control.BeginStoryboard(temp);
            } else {
                control.BeginStoryboard(sb);
            }
        }

        public static Storyboard FindResource(string name) {
            return (Storyboard)Application.Current.FindResource(name);
        }
    }
}
