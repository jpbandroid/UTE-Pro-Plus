using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UltraTextEdit.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableDialog : ContentDialog
    {
        public TableDialog()
        {
            this.InitializeComponent();
        }

        public int rows;
        public int columns;
        public int width;
        public int height;


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            variable_extraction();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public void variable_extraction()
        {
            
            rows = Int32.Parse(rowBox.Text);
            columns = Int32.Parse(columnBox.Text);
            if (defaultWidth.IsChecked == true)
            {
                width = 1000;
            } else
            {
                width = Int32.Parse(widthBox.Text);
            }
            if (defaultHeight.IsChecked == true)
            {
                height = 100;
            } else
            {
                height = Int32.Parse(heightBox.Text);
            }
        }
    }
}
