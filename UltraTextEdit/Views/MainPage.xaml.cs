using Microsoft.UI.Xaml.Controls;

using UltraTextEdit.ViewModels;

namespace UltraTextEdit.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
