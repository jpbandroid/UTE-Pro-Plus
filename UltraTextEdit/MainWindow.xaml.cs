using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UltraTextEdit.Views;
using Windows.Graphics;
using Windows.Storage;
using Windows.UI;
using WinUIEx;
using Colors = Microsoft.UI.Colors;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UltraTextEdit
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowEx window = this;
            window.SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.BaseAlt
            };
            var appWindow = AppWindow;
            var titleBar = appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;

            LoadBounds();
            AddTabItem();

            bool isTallTitleBar = true;
            if (AppWindowTitleBar.IsCustomizationSupported() && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 255, 255, 255);
                AppWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(25, 200, 200, 200);
                if (isTallTitleBar)
                {
                    appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
                }
                else
                {
                    appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
                }
            }

            window.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {

        }

        private void TabbedView_AddTabButtonClick(TabView sender, object args)
        {
            AddTabItem();
        }

        public void AddTabItem()
        {
            var TI = new TabViewItem();
            TI.Header = "New Document";
            TI.IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Document
            };
            var m_backdropController = new AcrylicBrush();
            m_backdropController.TintColor = Color.FromArgb(255, 0, 0, 0);
            m_backdropController.TintOpacity = 0.7;
            m_backdropController.TintLuminosityOpacity = 0.75;
            m_backdropController.Opacity = 0.3;

            var F = new Frame();
            F.Navigate(typeof(MainPage));
            TI.Content = F;

            TabbedView.Resources.TryAdd("TabViewItemHeaderBackgroundSelected", m_backdropController);
            TabbedView.TabItems.Add(TI);
            TabbedView.SelectedIndex = TabbedView.TabItems.IndexOf(TI);
            LoadBounds();
        }

        public void LoadBounds()
        {
            if (TabbedView.TabItems.Count > 1)
            {
                var appWindow = AppWindow;
                var titleBar = appWindow.TitleBar;
                // Create a RectInt32 object with the coordinates of TabStripFooter
                RectInt32 rect = new RectInt32((int)(AppWindow.Size.Width - AppTitleBar.ActualWidth + 250), 0, 9999, 45);
                // Create an array of RectInt32 objects with one element
                RectInt32[] rects = new RectInt32[1];

                // Add the rect object to the array
                rects[0] = rect;

                // Set the drag rectangles for the title bar
                titleBar.SetDragRectangles(rects);
                Debug.WriteLine(AppWindow.Size.Width);
                Debug.WriteLine(AppTitleBar.ActualWidth);
            }
            else
            {
                var appWindow = AppWindow;
                var titleBar = appWindow.TitleBar;
                // Create a RectInt32 object with the coordinates of TabStripFooter
                RectInt32 rect = new RectInt32(310, 0, 9999, 45);
                // Create an array of RectInt32 objects with one element
                RectInt32[] rects = new RectInt32[1];

                // Add the rect object to the array
                rects[0] = rect;

                // Set the drag rectangles for the title bar
                titleBar.SetDragRectangles(rects);
            }
        }

        public void AddTabItem(object args)
        {

        }
    }
}
