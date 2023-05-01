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
        public PrismUser PrismUser { get => prismUser; set => SetProperty(ref prismUser, value); }
        public IRelayCommand LoginCommand { get; set; }
        public bool ShowLoginPopup { get => showLoginPopup; set => SetProperty(ref showLoginPopup, value); }

        public MainViewModel(IHttpService httpServiceProxy)
        {
            httpService = httpServiceProxy;

            LoginCommand = new RelayCommand(Login, LoginCanExecute);

            CheckUser();
        }

        private void Login()
        {
            ShowLoginPopup = true;
        }

        private bool LoginCanExecute()
        {
            return PrismUser == null;
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
