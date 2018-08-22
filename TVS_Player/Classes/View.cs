using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace TVS_Player
{
    class View {
        private static List<Page> Pages { get; set; } = new List<Page>();
        private static int PagesOnTopCount { get; set; } = 0;

        static MainWindow Main { get; } = (MainWindow)Application.Current.MainWindow;

        public static void AddPage(Page page) {
            Grid baseGrid = new Grid();
            Grid hider = new Grid {
                Background = Helper.StringToBrush("#7000")
            };
            Frame content = new Frame() {
                Content = page,
                Margin = new Thickness(0, Main.ActualHeight, 0, Main.ActualHeight * -1)
            };
            baseGrid.Children.Add(hider);
            baseGrid.Children.Add(content);
            Main.TopContent.Children.Add(baseGrid);
            ((Storyboard)Main.FindResource("BlurBackground")).Begin();
            AnimateLocal(content, content.Margin, new Thickness(0));
            Animate.FadeIn(baseGrid);
            PagesOnTopCount++;
            HandleBackButton();
        }


        public static void RemovePage() {
            if (Main.TopContent.Children.Count > 0) {
                var grid = (Grid)Main.TopContent.Children[Main.TopContent.Children.Count - 1];
                Animate.FadeOut(grid);
                AnimateLocal((Frame)grid.Children[1], ((Frame)grid.Children[1]).Margin, new Thickness(0,Main.ActualHeight*-1,0,0),()=> {
                    Main.TopContent.Children.Remove(grid);

                });               
                PagesOnTopCount--;
                HandleBackButton();
            }
            if (Main.TopContent.Children.Count - 1 == 0) {
                ((Storyboard)Main.FindResource("UnBlurBackground")).Begin();
            }
        }

        public static void RemoveAllPages() {
        }

        public static void SetSearchBarVisibility(bool visible) {
            if (visible) {
                Main.SearchBar.Visibility = Visibility.Visible;
            } else {
                Main.SearchBar.Visibility = Visibility.Collapsed;
            }
        }

        public static void SetPage(Page page) {
            AddToPages((Page)Main.MainContent.Content);
            Main.MainContentOld.Source = RenderBitmap(Main.MainContent);
            Main.MainContent.Content = page;
            Panel.SetZIndex(Main.MainContent, 1);
            Panel.SetZIndex(Main.MainContentOld, 2);
            AnimateLocal(Main.MainContentOld, Main.MainContentOld.Margin, new Thickness(0, Main.ActualHeight * -1, 0, 0),()=> {
                Main.MainContentOld.Source = null;
                AnimateLocal(Main.MainContentOld, Main.MainContentOld.Margin, new Thickness(0));
            });
            Animate.FadeOut(Main.MainContentOld, async ()=> {
                await Task.Delay(500);
                Animate.FadeIn(Main.MainContentOld);
            });
        }


        public static void GoBack() {
            var page = Pages.Last();
            RemoveLast();
            Main.MainContentOld.Source = RenderBitmap(Main.MainContent);
            Panel.SetZIndex(Main.MainContent, 2);
            Panel.SetZIndex(Main.MainContentOld, 1);
            var anim = new ThicknessAnimation(new Thickness(0, Main.ActualHeight * -1, 0, 0), TimeSpan.FromMilliseconds(1));
            anim.Completed += (s, ev) => {
                Main.MainContent.Content = page;
                AnimateLocal(Main.MainContent, Main.MainContent.Margin, new Thickness(0), () => {
                    Main.MainContentOld.Source = null;
                });
                Animate.FadeIn(Main.MainContent);
            };
            Main.MainContent.BeginAnimation(Page.MarginProperty, anim);
        }

        public static void ClearHistory() {
            Pages.Clear();
            HandleBackButton();
        }

        private static void AddToPages(Page page) {
            Pages.Add(page);
            HandleBackButton();
        }

        private static void RemoveLast() {
            Pages.RemoveAt(Pages.Count - 1);
            HandleBackButton();
        }

        private static void HandleBackButton() {
            if (Pages.Count > 0 && PagesOnTopCount == 0) {
                ShowBackButton();
            } else {
                HideBackButton();
            }
        }

        private static void AnimateLocal(FrameworkElement element, Thickness start, Thickness end, Action completed = null) {
            ThicknessAnimation animation = new ThicknessAnimation {
                From = start,
                To = end,
                Duration = TimeSpan.FromMilliseconds(200),
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5,               
            };
            Timeline.SetDesiredFrameRate(animation, 60);
            if (completed != null) {
                animation.Completed += (s, ev) => completed();
            }
            element.BeginAnimation(Grid.MarginProperty, animation);
        }

        private static RenderTargetBitmap RenderBitmap(FrameworkElement element) {
            double topLeft = 0;
            double topRight = 0;
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            double dpi = 96;
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            dc.DrawRectangle(new VisualBrush(element), null, new Rect(topLeft, topRight, width, height));
            dc.Close();
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);
            bitmap.Render(visual);
            return bitmap;
        }

        private static void ShowBackButton() {
            Animate.FadeIn(Main.BackButton);
            Storyboard sb = new Storyboard();
            DoubleAnimation anim = new DoubleAnimation(33, TimeSpan.FromMilliseconds(200));
            sb.Children.Add(anim);
            Storyboard.SetTarget(anim, Main.BackButtonWidth);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(ColumnDefinition.MaxWidth)"));
            sb.Begin();
            Main.BackBar.BeginAnimation(Grid.WidthProperty, new DoubleAnimation(130, TimeSpan.FromMilliseconds(200)));
        }

        private static void HideBackButton() {
            Animate.FadeOut(Main.BackButton);
            Storyboard sb = new Storyboard();
            DoubleAnimation anim = new DoubleAnimation(7, TimeSpan.FromMilliseconds(200));
            sb.Children.Add(anim);
            Storyboard.SetTarget(anim, Main.BackButtonWidth);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(ColumnDefinition.MaxWidth)"));
            sb.Begin();
            Main.BackBar.BeginAnimation(Grid.WidthProperty, new DoubleAnimation(107, TimeSpan.FromMilliseconds(200)));
        }
    }
}
