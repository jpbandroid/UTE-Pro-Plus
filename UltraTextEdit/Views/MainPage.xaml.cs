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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using Microsoft.UI.Xaml.Markup;
using System.Text;
using CommunityToolkit.Common.Parsers.Markdown;

namespace UltraTextEdit.Views;

public sealed partial class MainPage : Page
{
    private const double V = 10.5;
    private const int V1 = 28;
    private bool saved = false;
    private string fileNameWithPath = "";

    public MainViewModel ViewModel
    {
        get;
    }

    public List<string> fonts
    {
        get
        {
            return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
        }
    }

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
        FileOpenPicker open = App.MainWindow.CreateOpenFilePicker();
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
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
            charFormatting.Strikethrough = FormatEffect.Toggle;
            selectedText.CharacterFormat = charFormatting;
        }
    }

    private async void DisplayAboutDialog()
    {
        ContentDialog aboutDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "UltraTextEdit Pro Plus",
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

    private void BackPicker_ColorChanged(object Sender, ColorChangedEventArgs EvArgs)
    {
        //Configure font highlight
        if (!(editor == null))
        {
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                _ = ST.CharacterFormat;
                var Br = new SolidColorBrush(BackPicker.Color);
                var CF = BackPicker.Color;
                if (BackAccent != null) BackAccent.Foreground = Br;
                ST.CharacterFormat.BackgroundColor = CF;
            }
        }
    }

    private void HighlightButton_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Configure font color
        var BTN = Sender as Button;
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            _ = ST.CharacterFormat.ForegroundColor;
            var Br = BTN.Foreground;
            BackAccent.Foreground = Br;
            ST.CharacterFormat.BackgroundColor = (BTN.Foreground as SolidColorBrush).Color;
        }
    }

    private void NullHighlightButton_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Configure font color
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            _ = ST.CharacterFormat.ForegroundColor;
            BackAccent.Foreground = new SolidColorBrush(Colors.Transparent);
            ST.CharacterFormat.BackgroundColor = Colors.Transparent;
        }
    }

    private void ColorButton_Click(object sender, RoutedEventArgs e)
    {
        // Extract the color of the button that was clicked.
        Button clickedColor = (Button)sender;
        var borderone = (Border)clickedColor.Content;
        var bordertwo = (Border)borderone.Child;
        var rectangle = (Rectangle)bordertwo.Child;
        var color = (rectangle.Fill as SolidColorBrush).Color;
        editor.Document.Selection.CharacterFormat.ForegroundColor = color;
        //FontColorMarker.SetValue(ForegroundProperty, new SolidColorBrush(color));
        editor.Focus(FocusState.Keyboard);
    }

    private void fontcolorsplitbutton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
    {
        // If you see this, remind me to look into the splitbutton color applying logic
    }

    private void ConfirmColor_Click(object sender, RoutedEventArgs e)
    {
        // Confirm color picker choice and apply color to text
        Color color = myColorPicker.Color;
        editor.Document.Selection.CharacterFormat.ForegroundColor = color;

        // Hide flyout
        colorPickerButton.Flyout.Hide();
    }

    private void CancelColor_Click(object sender, RoutedEventArgs e)
    {
        // Cancel flyout
        colorPickerButton.Flyout.Hide();
    }

    private void AppBarButton_Click2(object sender, RoutedEventArgs e)
    {
        ChangelogWindow changelog = new ChangelogWindow();
        changelog.Activate();
    }

    private async void ImageInsert_Click(object sender, RoutedEventArgs e)
    {
        FileOpenPicker open = App.MainWindow.CreateOpenFilePicker();
        open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        open.FileTypeFilter.Add(".png");
        open.FileTypeFilter.Add(".jpeg");
        open.FileTypeFilter.Add("*");

        Windows.Storage.StorageFile file = await open.PickSingleFileAsync();
        MainWindow window = new MainWindow();

        if (file != null)
        {
            using IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);
            var properties = await file.Properties.GetImagePropertiesAsync();
            int width = (int)properties.Width;
            int height = (int)properties.Height;

            ImageOptionsDialog dialog = new()
            {
                DefaultWidth = width,
                DefaultHeight = height,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                editor.Document.Selection.InsertImage((int)dialog.DefaultWidth, (int)dialog.DefaultHeight, 0, VerticalCharacterAlignment.Baseline, string.IsNullOrWhiteSpace(dialog.Tag) ? "Image" : dialog.Tag, randAccStream);
                return;
            }

            // Insert an image
            editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "Image", randAccStream);
        }
        //_wasOpen = true;
        //Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
        //Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
    }
    private void AddLinkButton_Click(object sender, RoutedEventArgs e)
        {
            hyperlinkText.AllowFocusOnInteraction = true;
            editor.Document.Selection.Link = $"\"{hyperlinkText.Text}\"";
            editor.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), "#6194c7");
            AddLinkButton.Flyout.Hide();
        }

    /* Method to create a table format string which can directly be set to 
   RichTextBoxControl. Rows, columns and cell width are passed as parameters 
   rather than hard coding as in previous example.*/
    private String InsertTableInRichTextBox(int rows, int cols, int width)
    {
        //Create StringBuilder Instance
        StringBuilder strTableRtf = new StringBuilder();

        //beginning of rich text format
        strTableRtf.Append(@"{\rtf1 ");

        //Variable for cell width
        int cellWidth;

        //Start row
        strTableRtf.Append(@"\trowd");

        //Loop to create table string
        for (int i = 0; i < rows; i++)
        {
            strTableRtf.Append(@"\trowd");

            for (int j = 0; j < cols; j++)
            {
                //Calculate cell end point for each cell
                cellWidth = (j + 1) * width;

                //A cell with width 1000 in each iteration.
                strTableRtf.Append(@"\cellx" + cellWidth.ToString());
            }

            //Append the row in StringBuilder
            strTableRtf.Append(@"\intbl \cell \row");
        }
        strTableRtf.Append(@"\pard");
        strTableRtf.Append(@"}");
        var strTableString = strTableRtf.ToString();
        editor.Document.Selection.SetText(TextSetOptions.FormatRtf, strTableString);
        return strTableString;

    }

    private async void AddTableButton_Click(object sender, RoutedEventArgs e)
    {
        var dialogtable = new TableDialog();
        dialogtable.XamlRoot = this.XamlRoot;
        await dialogtable.ShowAsync();
        InsertTableInRichTextBox(dialogtable.rows, dialogtable.columns, dialogtable.width);
    }

    private async void DateInsertionAsync(object sender, RoutedEventArgs e)
    { // Create a ContentDialog
        ContentDialog dialog = new ContentDialog();
        dialog.Title = "Insert current date and time";
        dialog.XamlRoot = this.XamlRoot;

        // Create a ListView for the user to select the date format
        ListView listView = new ListView();
        listView.SelectionMode = ListViewSelectionMode.Single;

        // Create a list of date formats to display in the ListView
        List<string> dateFormats = new List<string>();
        dateFormats.Add(DateTime.Now.ToString("dd.M.yyyy"));
        dateFormats.Add(DateTime.Now.ToString("M.dd.yyyy"));
        dateFormats.Add(DateTime.Now.ToString("dd MMM yyyy"));
        dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
        dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy"));
        dateFormats.Add(DateTime.Now.ToString("hh:mm:ss tt"));
        dateFormats.Add(DateTime.Now.ToString("HH:mm:ss"));
        dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy, HH:mm:ss"));
        dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss"));
        dateFormats.Add(DateTime.Now.ToString("MMM dd, yyyy"));

        // Set the ItemsSource of the ListView to the list of date formats
        listView.ItemsSource = dateFormats;

        // Set the content of the ContentDialog to the ListView
        dialog.Content = listView;

        // Make the insert button colored
        dialog.DefaultButton = ContentDialogButton.Primary;

        // Add an "Insert" button to the ContentDialog
        dialog.PrimaryButtonText = "OK";
        dialog.PrimaryButtonClick += (s, args) =>
        {
            string selectedFormat = listView.SelectedItem as string;
            string formattedDate = dateFormats[listView.SelectedIndex];
            editor.Document.Selection.Text = formattedDate;
        };

        // Add a "Cancel" button to the ContentDialog
        dialog.SecondaryButtonText = "Cancel";

        // Show the ContentDialog
        await dialog.ShowAsync();
    }
    #region Templates

    private void Template1_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Normal
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = (float)V;
            Combo3.Text = V.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template2_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Title
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            var PF = ST.ParagraphFormat;
            PF.Alignment = ParagraphAlignment.Center;
            CF.Bold = FormatEffect.Off;
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 28;
            Combo3.Text = V1.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template3_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Title 2
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            var PF = ST.ParagraphFormat;
            PF.Alignment = ParagraphAlignment.Center;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 22;
            Combo3.Text = 22.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template4_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Important
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.On;
            
            CF.Italic = FormatEffect.On;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 16;
            Combo3.Text = 16.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template5_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Header
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 14;
            Combo3.Text = 14.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template6_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Medium
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 18;
            Combo3.Text = 18.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template7_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Subtitle
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 20;
            Combo3.Text = 20.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template8_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Strong
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.On;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 18;
            Combo3.Text = 18.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template9_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Content
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 16;
            Combo3.Text = 16.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template10_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Finished
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            
            CF.Italic = FormatEffect.On;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 14;
            Combo3.Text = 14.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template11_Click(object Sender, RoutedEventArgs EvArgs)
    {
        //Unfinished
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.On;
            
            CF.Italic = FormatEffect.Off;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 14;
            Combo3.Text = 14.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }

    private void Template12_Click(object Sender, RoutedEventArgs EvArgs, Windows.UI.Text.FontStretch undefined)
    {
        //Strong header
        var ST = editor.Document.Selection;
        if (!(ST == null))
        {
            var CF = ST.CharacterFormat;
            CF.Bold = FormatEffect.Off;
            CF.Italic = FormatEffect.On;
            CF.Name = "Segoe UI";
            FontBox.SelectedItem = "Segoe UI";

            CF.Outline = FormatEffect.Off;
            CF.Size = 18;
            CF.ForegroundColor = Colors.DimGray;
            Combo3.Text = 18.ToString();
            CF.Underline = UnderlineType.None;
            ST.CharacterFormat = CF;
            TempFlyout.Hide();
        }
    }


    #endregion Templates

    private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            // Apply the list style to the selected text.
            var paragraphFormatting = selectedText.ParagraphFormat;
            paragraphFormatting.Alignment = ParagraphAlignment.Left;

        }
    }

    private void AlignCenterButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            // Apply the list style to the selected text.
            var paragraphFormatting = selectedText.ParagraphFormat;
            paragraphFormatting.Alignment = ParagraphAlignment.Center;

        }
    }

    private void AlignRightButton_Click(object sender, RoutedEventArgs e)
    {
        ITextSelection selectedText = editor.Document.Selection;
        if (selectedText != null)
        {
            // Apply the list style to the selected text.
            var paragraphFormatting = selectedText.ParagraphFormat;
            paragraphFormatting.Alignment = ParagraphAlignment.Right;

        }
    }
}
