using RMSreader.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace RMSreader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif
        public static StorageFile importedPdfFile;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        ContinuationManager continuationManager;
        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            continuationManager = new ContinuationManager();

            Frame rootFrame = CreateRootFrame();

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                Frame scenarioFrame = Window.Current.Content as Frame;
                if (scenarioFrame != null)
                {
                    // Call ContinuationManager to handle continuation activation
                    continuationManager.Continue(continuationEventArgs, scenarioFrame);
                }
            }

            Window.Current.Activate();
        }

        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }   

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            var page = new MainPage();

            // Get file from args
            var file = (args.Files[0] as StorageFile);
            bool fileValid = true;

            if (!file.FileType.Equals(".ppdf"))
                fileValid = false;
            else
            {
                fileValid = await GetContentFromStorageFile(file);
            }

            if (!fileValid)
            {
                ShowErrorMessageAndClose();
            }
            else
            {
                LocalStorage.SaveSetting("lastFileName", file.Name);

                importedPdfFile = file;
            }

            // Navigate to individuell page
            Window.Current.Content = page;
            Window.Current.Activate();
        }

        public static async Task<bool> GetContentFromStorageFile(StorageFile file)
        {
            // Get content from file
            var randomAccessStream = await file.OpenReadAsync();
            Stream stream = randomAccessStream.AsStreamForRead();
            //StreamReader streamReader = new StreamReader(stream);
            //var fileContent = await streamReader.ReadToEndAsync();

            byte[] bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            IBuffer iBuff = bytes.AsBuffer();

            try
            {
                var test = await Microsoft.RightsManagement.UserPolicy.AcquireAsync(iBuff, "Moritz.Heilmann@henkel.com", new MRMAuthCallBack(), new MRMConsentCallback(), Microsoft.RightsManagement.PolicyAcquisitionOptions.None);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            if (stream == null)
                return false;

            return true;
        }

        private async void ShowErrorMessageAndClose()
        {
            var messageDialog = new MessageDialog("Das Dokument hatte entweder ein falsches Format oder enthielt Fehler. Ein Import kann erneut versucht werden und die Applikation wird geschlossen.", "Fehlerhafter Import");
            messageDialog.Commands.Add(new UICommand("Schließen", command =>
            {
                App.Current.Exit();
            }));
            await messageDialog.ShowAsync();
        }

        public static async void ShowErrorDialog(string message, string title)
        {
            var messageDialog = new MessageDialog(message, title);
            messageDialog.Commands.Add(new UICommand("Ok"));
            await messageDialog.ShowAsync();
        }


    }
}

class MRMAuthCallBack : Microsoft.RightsManagement.IAuthenticationCallback
{
    #region IAuthenticationCallback Members

    IAsyncOperation<string> Microsoft.RightsManagement.IAuthenticationCallback.GetTokenAsync(Microsoft.RightsManagement.AuthenticationParameters authenticationParameters)
    {
        throw new NotImplementedException();
    }

    #endregion
}

class MRMConsentCallback : Microsoft.RightsManagement.IConsentCallback
{

    #region IConsentCallback Members

    public IAsyncOperation<IEnumerable<Microsoft.RightsManagement.IConsent>> ConsentsAsync(IEnumerable<Microsoft.RightsManagement.IConsent> consents)
    {
        throw new NotImplementedException();
    }

    #endregion
}