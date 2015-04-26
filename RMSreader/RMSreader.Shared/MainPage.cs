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
using Windows.Data;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RMSreader
{
    public sealed partial class MainPage : Page, IWebAuthenticationContinuable
    {
        private StorageFile storageFile;
        public Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContext;
        public Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authResult;
        private CoreApplicationView view = CoreApplication.GetCurrentView();

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                lastFileName.Text = Convert.ToString(LocalStorage.GetSetting("lastFileName"));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void login_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        public async Task Login()
        {
            try
            {
                authContext = await Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext.CreateAsync("https://login.windows.net/appsthepagedot.onmicrosoft.com", true);
                authContext.AcquireTokenAndContinue("   https://graph.windows.net/", "eaba3c4d-f622-4063-830d-bd75cda400f5", new Uri("http://www.google.de"), authenticationContextDelegate);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void authenticationContextDelegate(Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result)
        {
            authResult = result;

            if (result.Status == Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationStatus.Success)
            {
                Debug.WriteLine(authResult.AccessToken.ToString());
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine(authResult.Status.ToString());
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine(authResult.AccessTokenType.ToString());
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine("-----------------------------------------");
                Debug.WriteLine(authResult.UserInfo.GivenName.ToString());
            }
        }
        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            await authContext.ContinueAcquireTokenAsync(args);
        }
    }
}
