using RMSreader.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace RMSreader
{
    public sealed partial class MainPage
    {
        private StorageFile storageFile;
        private CoreApplicationView view = CoreApplication.GetCurrentView();

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
                lastFileName.Text = Convert.ToString(LocalStorage.GetSetting("lastFileName"));
        }
    }
}
