using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Page {
        public AboutView() {
            InitializeComponent();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void FlatIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/");
        }
        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/fermam-aziz");
        }

        private void TextBlock_MouseUp_1(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/cogwheel-outline_57818");
        }

        private void TextBlock_MouseUp_2(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/smashicons");
        }

        private void TextBlock_MouseUp_3(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/windows_149232");
        }

        private void TextBlock_MouseUp_4(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/freepik");
        }

        private void TextBlock_MouseUp_5(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/fullscreen_130909");
        }

        private void TextBlock_MouseUp_6(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/gregor-cresnar");
        }

        private void TextBlock_MouseUp_7(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/download_126488");
        }

        private void TextBlock_MouseUp_8(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/pause_149659");
        }

        private void TextBlock_MouseUp_9(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/anatoly");
        }

        private void TextBlock_MouseUp_10(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/import_243718");
        }

        private void TextBlock_MouseUp_11(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/anton-saputro");
        }

        private void TextBlock_MouseUp_12(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/broadcast_89571");
        }

        private void TextBlock_MouseUp_13(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/sort_585856");
        }

        private void TextBlock_MouseUp_14(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/calendar_149363");
        }

        private void TextBlock_MouseUp_15(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/list_159841");
        }

        private void TextBlock_MouseUp_16(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/nikita-golubev");
        }

        private void TextBlock_MouseUp_17(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/poster_291101");
        }

        private void TextBlock_MouseUp_18(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/paint-brush-and-palette_107810");
        }

        private void TextBlock_MouseUp_19(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/lucy-g");
        }

        private void TextBlock_MouseUp_20(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/edit_118779");
        }

        private void TextBlock_MouseUp_21(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/lyolya");
        }

        private void TextBlock_MouseUp_22(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/plus-symbol_109615");
        }

        private void TextBlock_MouseUp_23(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/letter-x_109602");
        }

        private void TextBlock_MouseUp_24(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/new-file_109705");
        }

        private void TextBlock_MouseUp_25(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/play-button_109720");
        }

        private void TextBlock_MouseUp_26(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/check-square_109730");
        }

        private void TextBlock_MouseUp_27(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/right-arrow_118740");
        }

        private void TextBlock_MouseUp_28(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/more_149976");
        }

        private void TextBlock_MouseUp_29(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/magnifying-glass_126474");
        }

        private void TextBlock_MouseUp_30(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/folder_126480");
        }

        private void TextBlock_MouseUp_31(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/reload_126502");
        }

        private void TextBlock_MouseUp_32(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/authors/chris-veigt");
        }

        private void TextBlock_MouseUp_33(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/menu_131932");
        }

        private void TextBlock_MouseUp_34(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/faq_88274");
        }

        private void TextBlock_MouseUp_35(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.flaticon.com/free-icon/checked_121715");
        }

        private void TextBlock_MouseUp_36(object sender, MouseButtonEventArgs e) {
            Process.Start("https://github.com/psantosl/ConsoleToast");
        }

        private void TextBlock_MouseUp_37(object sender, MouseButtonEventArgs e) {
            Process.Start("https://github.com/psantosl");
        }

        private void TextBlock_MouseUp_38(object sender, MouseButtonEventArgs e) {
            Process.Start("https://ikriv.com/");
        }


        private void TextBlock_MouseUp_40(object sender, MouseButtonEventArgs e) {
            Process.Start("https://www.ikriv.com/dev/wpf/MathConverter/");
        }

        private void TextBlock_MouseUp_39(object sender, MouseButtonEventArgs e) {
            Process.Start("https://github.com/morelinq/MoreLINQ");
        }
    }
}
