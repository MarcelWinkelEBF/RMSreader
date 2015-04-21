using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RMSreader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void loadLocalFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            filePicker.ViewMode = PickerViewMode.List;

            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".ppdf");

            filePicker.PickSingleFileAndContinue();

            view.Activated += viewActivated;
        }

        private async void viewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            FileOpenPickerContinuationEventArgs arguments = args as FileOpenPickerContinuationEventArgs;
            if (arguments != null)
            {
                if (arguments.Files.Count == 0) return;

                view.Activated += viewActivated;
                storageFile = arguments.Files[0];
                bool validFile = await App.GetContentFromStorageFile(storageFile);

                if (!validFile)
                {
                    var messageDialog = new MessageDialog("Das Dokument hatte entweder ein falsches Format oder enthielt Fehler. Ein Import kann erneut versucht werden und die Applikation wird geschlossen.", "Fehlerhafter Import");
                    messageDialog.Commands.Add(new UICommand("Ok"));
                    await messageDialog.ShowAsync();
                }
            }
        }
    }
}
