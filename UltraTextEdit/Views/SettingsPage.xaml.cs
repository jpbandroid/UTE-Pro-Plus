using Microsoft.UI.Xaml.Controls;

using UltraTextEdit.ViewModels;

namespace UltraTextEdit.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void GH_Navigate(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // The URI to launch
        string uriToLaunch = @"https://github.com/jpbandroid/UTE-Pro-Plus";

        // Create a Uri object from a URI string 
        var uri = new Uri(uriToLaunch);

        // Launch the URI
        async void DefaultLaunch()
        {
            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
            }
        }
        DefaultLaunch();
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }
}
