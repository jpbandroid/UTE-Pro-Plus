using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using UltraTextEdit.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Microsoft.UI.Text;
using Windows.Foundation.Metadata;

namespace UltraTextEdit.Views;

public sealed partial class MainPage : Page
{
    private bool saved = false;
    private string fileNameWithPath = "";

    public MainViewModel ViewModel
    {
        get;
    }

    public List<string> fonts => CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();

    public List<double> FontSizes
    {
        get;
    } = new List<double>()
            {
                8,
                9,
                10,
                11,
                12,
                14,
                16,
                18,
                20,
                24,
                28,
                36,
                48,
                72,
                96};

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        //SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
    }

    private void RichEditBox_TextChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void OnKeyboardAcceleratorInvoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {

    }

    private void ComboBox_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Microsoft.UI.Text.ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            // Get the instance of ComboBox
            ComboBox? fontbox = sender as ComboBox;

            // Get the ComboBox selected item text
            var selectedItems = fontbox.SelectedItem.ToString();
            editor.Document.Selection.CharacterFormat.Name = selectedItems;
        }
    }

    private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
    {
        if (!saved) { e.Handled = true; ShowUnsavedDialog(); }
    }
    private async void ShowUnsavedDialog()
    {
        string fileName = "Your document";
        ContentDialog aboutDialog = new ContentDialog()
        {
            Title = "Do you want to save changes to " + fileName + "?",
            Content = "There are unsaved changes to your document, want to save them?",
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Save changes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Primary
        };

        ContentDialogResult result = await aboutDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            SaveFile(false);
        }
        else if (result == ContentDialogResult.Secondary)
        {
            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }
        else
        {
            // Do nothing
        }
    }
    private async void SaveFile(bool isCopy)
    {
        MainWindow window = new MainWindow();
        string fileName = "Untitled";
        if (isCopy || fileName == "Untitled")
        {
            FileSavePicker savePicker = App.MainWindow.CreateSaveFilePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we
                // finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    if (file.Name.EndsWith(".txt"))
                    {
                        editor.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.None, randAccStream);
                    }
                    else
                    {
                        editor.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                    }

                // Let Windows know that we're finished changing the file so the
                // other app can update the remote version of the file.
                //FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                //if (status != FileUpdateStatus.Complete)
                //{
                //    Windows.UI.Popups.MessageDialog errorBox = new("File " + file.Name + " couldn't be saved.");
                //    await errorBox.ShowAsync();
                //}
                saved = true;
                fileNameWithPath = file.Path;
                //window.AppTitle.Text = file.Name + " - " + appTitleStr;
                //Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            }
        }
        else if (!isCopy || fileName != "Untitled")
        {
            //string path = fileNameWithPath.Replace("\\" + fileName, "");
            try
            {
                StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        if (file.Name.EndsWith(".txt"))
                        {
                            editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                        }
                        else
                        {
                            editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                        }


                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox = new("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    //window.AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                }
            }
            catch (Exception)
            {
                SaveFile(true);
            }
        }
    }

    private async void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        // Open a text file.
        Windows.Storage.Pickers.FileOpenPicker open = App.MainWindow.CreateOpenFilePicker();
        open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        open.FileTypeFilter.Add(".rtf");
        open.FileTypeFilter.Add(".txt");

        Windows.Storage.StorageFile file = await open.PickSingleFileAsync();
        MainWindow window = new MainWindow();

        if (file != null)
        {
            using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(file);
                var reader = DataReader.FromBuffer(buffer);
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string text = reader.ReadString(buffer.Length);
                // Load the file into the Document property of the RichEditBox.
                editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                //editor.Document.SetText(TextSetOptions.FormatRtf, text);
                //window.AppTitle.Text = file.Name + " - " + appTitleStr;
                fileNameWithPath = file.Path;
            }
            saved = true;
            //_wasOpen = true;
            //Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            //Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFile(true);
    }

    private void Combo3_Loaded(object sender, RoutedEventArgs e)
    {
        Combo3.SelectedIndex = 2;

        if ((ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)))
        {
            Combo3.TextSubmitted += Combo3_TextSubmitted;
        }
    }

    private void Combo3_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            bool isDouble = double.TryParse(sender.Text, out double newValue);

            // Set the selected item if:
            // - The value successfully parsed to double AND
            // - The value is in the list of sizes OR is a custom value between 8 and 100
            if (isDouble && (FontSizes.Contains(newValue) || (newValue < 100 && newValue > 8)))
            {
                // Update the SelectedItem to the new value. 
                sender.SelectedItem = newValue;
                editor.Document.Selection.CharacterFormat.Size = (float)newValue;
            }
            else
            {
                // If the item is invalid, reject it and revert the text. 
                sender.Text = sender.SelectedValue.ToString();

                var dialog = new ContentDialog
                {
                    Content = "The font size must be a number between 8 and 100.",
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Close
                };
                var task = dialog.ShowAsync();
            }
        }

        // Mark the event as handled so the framework doesn’t update the selected item automatically. 
        args.Handled = true;
    }

    private void BoldButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
            charFormatting.Bold = FormatEffect.Toggle;
            selectedText.CharacterFormat = charFormatting;
        }
    }

    private void ItalicButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
            charFormatting.Italic = FormatEffect.Toggle;
            selectedText.CharacterFormat = charFormatting;
        }
    }

    private void UnderlineButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
            if (charFormatting.Underline == UnderlineType.None)
            {
                charFormatting.Underline = UnderlineType.Single;
            }
            else
            {
                charFormatting.Underline = UnderlineType.None;
            }
            selectedText.CharacterFormat = charFormatting;
        }
    }

    private void StrikeButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BoldButton_Off(object sender, RoutedEventArgs e)
    {

    }

    private void ItalicButton_Off(object sender, RoutedEventArgs e)
    {

    }

    private void UnderlineButton_Off(object sender, RoutedEventArgs e)
    {

    }

    private void SuperscriptCheck(object sender, RoutedEventArgs e)
    {

    }

    private void SubscriptCheck(object sender, RoutedEventArgs e)
    {

    }

    private void StrikeButton_Off(object sender, RoutedEventArgs e)
    {

    }

    private async void DisplayAboutDialog()
    {
        ContentDialog aboutDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "UltraTextEdit",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close,
            Content = new VersionDialog()

    };

        await aboutDialog.ShowAsync();
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        DisplayAboutDialog();
    }
}
