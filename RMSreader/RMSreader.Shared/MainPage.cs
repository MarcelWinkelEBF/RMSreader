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
using Windows.UI.Xaml.Input;

namespace RMSreader
{
    public sealed partial class MainPage : Page, IWebAuthenticationContinuable
    {
        private StorageFile storageFile;
        public Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContext;
        public Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authResult;
        private CoreApplicationView view = CoreApplication.GetCurrentView();
        private const string clientId = "eaba3c4d-f622-4063-830d-bd75cda400f5";

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
                authContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext.CreateAsync("https://login.windows.net/appsthepagedot.onmicrosoft.com", true).GetResults();
                lastFileName.Text = Convert.ToString(LocalStorage.GetSetting("lastFileName"));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var mailId = Convert.ToString(LocalStorage.GetSetting("mailId"));
            var name = Convert.ToString(LocalStorage.GetSetting("name"));

            if (!String.IsNullOrEmpty(mailId))
            {
                authResult = await authContext.AcquireTokenSilentAsync("https://login.windows.net/appsthepagedot.onmicrosoft.com", clientId);

                if (authResult.Status == Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationStatus.ClientError)
                {
                    var refreshToken = Convert.ToString(LocalStorage.GetSetting("refreshToken"));
                    if (!String.IsNullOrEmpty(refreshToken)) {
                        authResult = await authContext.AcquireTokenByRefreshTokenAsync(refreshToken, clientId);
                        SaveTokenLocal();
                    } else {
                        authContext.AcquireTokenAndContinue("https://graph.windows.net/", clientId, new Uri("http://www.google.de"), authenticationContextDelegate);
                    }
                }

                loginButton.Visibility = Visibility.Collapsed;
                logoutButton.Visibility = Visibility.Visible;
                userName.Text = "Hallo, " + name;
                var accessToken = LocalStorage.GetSetting("accessToken");
            }
            else
            {
                logoutButton.Visibility = Visibility.Collapsed;
                loginButton.Visibility = Visibility.Visible;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }

        private void Logout()
        {
            if (authContext != null && authContext.TokenCache != null)
                authContext.TokenCache.Clear();
            if (authResult != null)
                authResult = null;


            LocalStorage.RemoveSetting("mailId");
            LocalStorage.RemoveSetting("name");

            LocalStorage.RemoveSetting("accessToken");
            LocalStorage.RemoveSetting("refreshToken");

            logoutButton.Visibility = Visibility.Collapsed;
            loginButton.Visibility = Visibility.Visible;

            userName.Text = "Nicht eigneloggt!";
        }

        public void Login()
        {
            try
            {
                authContext.AcquireTokenAndContinue("https://graph.windows.net/", clientId, new Uri("http://www.google.de"), authenticationContextDelegate);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void authenticationContextDelegate(Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result)
        {
            authResult = result;
            CheckAuthResultStatus(authResult);
        }

        private void CheckAuthResultStatus(Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authenticationResult)
        {
            switch (authenticationResult.Status)
            {
                case Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationStatus.Success:
                    SaveTokenLocal();
                    loginButton.Visibility = Visibility.Collapsed;
                    logoutButton.Visibility = Visibility.Visible;
                    userName.Text = "Hallo, " + authenticationResult.UserInfo.GivenName;
                    break;
                case Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationStatus.ServiceError:
                    App.ShowErrorDialog("Die Anmeldung am Server ist fehlgeschlagen. Überprüfen Sie Ihre Eingaben und versuchen Sie es erneut.", "Fehler");
                    break;
                case Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationStatus.ClientError:
                    App.ShowErrorDialog("Die Anmeldung am Server ist fehlgeschlagen, da es einen Fehler in der Applikation gab. Bitte versuchen Sie die Anmeldung erneut.", "Fehler");
                    break;
            }
        }

        private void SaveTokenLocal()
        {
            // if we refresh a token instead of generating a new one userInfo is not given by the service
            if (authResult.UserInfo != null)
            {
                LocalStorage.SaveSetting("mailId", authResult.UserInfo.DisplayableId);
                LocalStorage.SaveSetting("name", authResult.UserInfo.GivenName);
            }

            LocalStorage.SaveSetting("accessToken", authResult.AccessToken);
            LocalStorage.SaveSetting("refreshToken", authResult.RefreshToken);
        }

        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            await authContext.ContinueAcquireTokenAsync(args);
        }
    }
}
