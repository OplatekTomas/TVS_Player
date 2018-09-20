using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TVS_Player_Base;

namespace TVS_Player {

    class ViewCustomization {
        /// <summary>
        /// true - always, false - hidden, null - normal
        /// </summary>
        public bool? SearchBarVisible { get; set; }
    }

    class View {

        private static List<Page> Pages { get; set; } = new List<Page>();
        private static int PagesOnTopCount { get; set; } = 0;
        public static Type CurrentPage { get; set; }

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
            SetPageCustomization(_defaultCustomization);
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
            SetPageCustomization(_defaultCustomization);
        }

        public static async Task RemoveAllPages() {
        }


        public static void SetPage(Page page) {
            Pages.Add((Page)Main.MainContent.Content);
            HandleBackButton();
            Main.MainContentOld.Source = Helper.RenderElement(Main.MainContent);
            Main.MainContent.Content = page;
            CurrentPage = page.GetType();
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
            SetPageCustomization(_defaultCustomization);          
        }



        public static void GoBack() {
            var page = Pages.Last();
            RemoveLast();
            Main.MainContentOld.Source = Helper.RenderElement(Main.MainContent);
            Panel.SetZIndex(Main.MainContent, 2);
            Panel.SetZIndex(Main.MainContentOld, 1);
            var anim = new ThicknessAnimation(new Thickness(0, Main.ActualHeight * -1, 0, 0), TimeSpan.FromMilliseconds(1));
            anim.Completed += (s, ev) => {
                Main.MainContent.Content = page;
                CurrentPage = page.GetType();
                AnimateLocal(Main.MainContent, Main.MainContent.Margin, new Thickness(0), () => {
                    Main.MainContentOld.Source = null;
                });
                Animate.FadeIn(Main.MainContent);
            };
            Main.MainContent.BeginAnimation(Page.MarginProperty, anim);
            SetPageCustomization(_defaultCustomization);
        }


        public static void RenderSearchResult(HashSet<SearchResult> results) {
            results = results.OrderByDescending(x => x.Type).ToHashSet();
            if (results.Count > 25) {
                results = results.Take(25).ToHashSet();
            }
            Main.SearchResultPanel.Children.Clear();
            foreach (var item in results) {
                SearchResultControl result = new SearchResultControl();
                result.MouseLeftButtonUp += async (s, ev) => {
                    if (result.ResultType.Text == "Series") {
                        View.SetPage(new SeriesView(await Series.GetSeries(item.SeriesId)));
                    } else {
                        View.SetPage(new SeriesView(await Series.GetSeries(item.SeriesId), (int)(await Episode.GetEpisode(item.SeriesId, item.EpisodeId)).AiredSeason));
                    }
                    TopBar_MouseLeave();
                };
                result.ResultName.Text = item.Name;
                result.ResultType.Text = item.Type;
                Main.SearchResultPanel.Children.Add(result);
            }
        }
        public static void AnimateSearchResult(int count) {
            if (count == 0) {
                Animate.FadeOut(Main.SearchResultBar);
                Main.SearchResultBar.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(new Thickness(5, -1, 5, 0), TimeSpan.FromMilliseconds(100)));
            } else if (count > 0 && count < 6) {
                Main.SearchResultBar.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(new Thickness(5, -1, 5, -17 + (-50 * count)), TimeSpan.FromMilliseconds(100)));
                Main.SearchResultBar.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)));
            } else {
                Main.SearchResultBar.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)));
                Main.SearchResultBar.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(new Thickness(5, -1, 5, -250), TimeSpan.FromMilliseconds(100)));
            }
        }


        public static readonly ViewCustomization _defaultCustomization = new ViewCustomization() {
            SearchBarVisible = null
        };
        public static void SetPageCustomization(ViewCustomization custom) {
            Main.CenterPartTopBar.MouseEnter -= TopBar_MouseEnter;
            Main.CenterPartTopBar.MouseLeave -= TopBar_MouseLeave;
            switch (custom.SearchBarVisible) {
                case true:
                    TopBar_MouseEnter();
                    break;
                case false:
                    TopBar_MouseLeave();
                    break;
                case null:
                    Main.CenterPartTopBar.MouseEnter += TopBar_MouseEnter;
                    Main.CenterPartTopBar.MouseLeave += TopBar_MouseLeave;
                    break;
            }

        }

        private static void TopBar_MouseEnter(object sender, MouseEventArgs args) => TopBar_MouseEnter();

        private static void TopBar_MouseLeave(object sender, MouseEventArgs args) => TopBar_MouseLeave();

        private static void TopBar_MouseEnter() {
            Main.SearchBar.BeginStoryboard((Storyboard)Main.FindResource("FadeInSearchBar"));
        }

        private static void TopBar_MouseLeave() {
            Main.SearchBar.BeginStoryboard((Storyboard)Main.FindResource("FadeOutSearchBar"));
            Main.SearchBarText.Text = "";
            Keyboard.ClearFocus();
        }

        public static void ClearHistory() {
            Pages.Clear();
            HandleBackButton();
        }

        private static void RemoveLast() {
            Pages.RemoveAt(Pages.Count - 1);
            HandleBackButton();
        }

        private static void HandleBackButton() {
            Storyboard sb;
            if (Pages.Count > 0 && PagesOnTopCount == 0) {
                sb = (Storyboard)Main.FindResource("FadeInSearchBar");
            } else {
                sb = (Storyboard)Main.FindResource("FadeOutSearchBar");
            }
            Main.BackButton.BeginStoryboard(sb);

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
    }
}
