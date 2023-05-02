using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Prism_Drive.Models;
using Prism_Drive.Services;

namespace Prism_Drive.ViewModels
{
    internal class MainViewModel : ObservableObject
    {




        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }
        public PrismUser PrismUser { get => prismUser; set { SetProperty(ref prismUser, value); SaveUser(); CommandCanExecuteChanged(LoginCommand, LogoutCommand); } }

        private void SaveUser()
        {
            if (PrismUser == null)
            {
                Preferences.Default.Remove(AVATAR_KEY);
                Preferences.Default.Remove(NAME_KEY);
                Preferences.Default.Remove(ACCESS_TOKEN_KEY);
            }
            else
            {
                Preferences.Default.Set(AVATAR_KEY, PrismUser.AvatarUrl);
                Preferences.Default.Set(NAME_KEY, PrismUser.DisplayName);
                Preferences.Default.Set(ACCESS_TOKEN_KEY, PrismUser.AccessToken);
            }
        }

        public IRelayCommand LoginCommand { get; set; }
        public IRelayCommand LogoutCommand { get; set; }
        public bool ShowLoginPopup { get => showLoginPopup; set => SetProperty(ref showLoginPopup, value); }

        public MainViewModel(IHttpService httpServiceProxy)
        {
            httpService = httpServiceProxy;

            LoginCommand = new RelayCommand(Login, LoginCanExecute);
            LogoutCommand = new RelayCommand(Logout, LogoutCanExecute);

            CheckUser();
        }

        private static void CommandCanExecuteChanged(params IRelayCommand[] commands)
        {
            foreach (var command in commands)
            {
                command.NotifyCanExecuteChanged();
            }
        }

        private void Logout()
        {
            PrismUser = null;
        }

        private bool LogoutCanExecute()
        {
            return PrismUser != null && IsBusy == false;
        }

        private void Login()
        {
            ShowLoginPopup = true;
        }

        private bool LoginCanExecute()
        {
            return PrismUser == null && IsBusy == false;
        }

        private void CheckUser()
        {
            var hasAvatar = Preferences.Default.ContainsKey(AVATAR_KEY);
            var hasName = Preferences.Default.ContainsKey(NAME_KEY);
            var hasAcccessToken = Preferences.Default.ContainsKey(ACCESS_TOKEN_KEY);

            if (hasAvatar && hasName && hasAcccessToken)
            {
                PrismUser = new PrismUser
                {
                    DisplayName = Preferences.Default.Get(NAME_KEY, string.Empty),
                    AvatarUrl = Preferences.Default.Get(AVATAR_KEY, string.Empty),
                    AccessToken = Preferences.Default.Get(ACCESS_TOKEN_KEY, string.Empty)
                };
            }
            else
            {
                PrismUser = null;
            }
        }


        private bool isBusy = false;
        private readonly IHttpService httpService;
        private PrismUser prismUser;
        private bool showLoginPopup;

        private static readonly string AVATAR_KEY = "email";
        private static readonly string NAME_KEY = "name";
        private static readonly string ACCESS_TOKEN_KEY = "accessToken";
    }
}
