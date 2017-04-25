using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace TVSPlayer {

    public static class ThemeSwitcher {
        private static bool theme = false;

        public static void SwitchTheme() {
            Action a = () => Elapsed();
            Thread t = new Thread(a.Invoke);
            t.Start();
        }

        private static void Elapsed() {
            if (theme) {
                Application.Current.Resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242424"));
                Application.Current.Resources["ReverseBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                Application.Current.Resources["LighterBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#323232"));
                Application.Current.Resources["LightestBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#484848"));
                Application.Current.Resources["TransparentBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DD101010"));
                LightIcons();
                theme = false;
            } else {
                Application.Current.Resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
                Application.Current.Resources["ReverseBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242424"));
                Application.Current.Resources["LighterBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8C8C8"));
                Application.Current.Resources["LightestBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dcdcdc"));
                Application.Current.Resources["TransparentBG"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDE1E1E1"));

                DarkIcons();
                theme = true;
            }
        }

        public static void DarkIcons() {
            Application.Current.Resources["MenuIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-menu-dark.png", UriKind.Absolute));
            Application.Current.Resources["MoreIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-more-dark.png", UriKind.Absolute));
            Application.Current.Resources["SearchIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-search-dark.png", UriKind.Absolute));
            Application.Current.Resources["PlayIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-play-dark.png", UriKind.Absolute));
        }

        private static void LightIcons() {
            Application.Current.Resources["MenuIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-menu-light.png", UriKind.Absolute));
            Application.Current.Resources["MoreIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-more-light.png", UriKind.Absolute));
            Application.Current.Resources["SearchIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-search-light.png", UriKind.Absolute));
            Application.Current.Resources["PlayIcon"] = new BitmapImage(new Uri("pack://application:,,,/Icons/ico-play-light.png", UriKind.Absolute));
        }
    }
}