using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism_Drive.Models;
using Prism_Drive.Services;
using System.Diagnostics;

namespace Prism_Drive.ViewModels
{
    internal class MainViewModel : ObservableObject
    {




        public bool IsBusy { get => isBusy; set
            {
                SetProperty(ref isBusy, value);
                CommandCanExecuteChanged(LoginCommand, LogoutCommand);
                AsyncCommandCanExecuteChanged(CreateFolderCommand);
                Status = IsBusy ? "Working..." : "Ready";
            }
        }
        public PrismUser PrismUser { get => prismUser; 
            set 
            { 
                SetProperty(ref prismUser, value); 
                SaveUser(); 
                CommandCanExecuteChanged(LoginCommand, LogoutCommand); 
                AsyncCommandCanExecuteChanged(CreateFolderCommand);
            } 
        }

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
        public IAsyncRelayCommand CreateFolderCommand { get; set; }
        public bool ShowLoginPopup { get => showLoginPopup; set => SetProperty(ref showLoginPopup, value); }
        public string NewFolderName { get => newFolderName; set { SetProperty(ref newFolderName, value); AsyncCommandCanExecuteChanged(CreateFolderCommand); } }

        public string Status { get => status; set => SetProperty(ref status, value); }
        public string LastOperation { get => lastOperation; set => SetProperty(ref lastOperation, value); }

        public MainViewModel(IHttpService httpServiceProxy)
        {
            httpService = httpServiceProxy;

            LoginCommand = new RelayCommand(Login, LoginCanExecute);
            LogoutCommand = new RelayCommand(Logout, LogoutCanExecute);
            CreateFolderCommand = new AsyncRelayCommand(CreateFolder, CreateFolderCanExecute);

            CheckUser();

            Status = "Ready";
        }

        private bool CreateFolderCanExecute()
        {
            return PrismUser != null && IsBusy == false && string.IsNullOrWhiteSpace(NewFolderName) == false;
        }

        private async Task CreateFolder()
        {
            IsBusy = true;

            var result = await httpService.CreateFolderAsync(NewFolderName, PrismUser.AccessToken);

            if (result)
            {
                LastOperation = $"{NewFolderName} created successfully. ({DateTime.Now})";
            }
            else
            {
                LastOperation = $"Failed to create {NewFolderName}. ({DateTime.Now})";
            }

            IsBusy = false;

        }

        private static void CommandCanExecuteChanged(params IRelayCommand[] commands)
        {
            foreach (var command in commands)
            {
                command.NotifyCanExecuteChanged();
            }
        }

        private static void AsyncCommandCanExecuteChanged(params IAsyncRelayCommand[] commands)
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

        public async Task GetFileList()
        {
            var fileList = await httpService.GetFileListsAsync(PrismUser.AccessToken);
            Debug.WriteLine($"\n\n\n{fileList}\n\n\n");
        }

        private bool isBusy = false;
        private readonly IHttpService httpService;
        private PrismUser prismUser;
        private bool showLoginPopup;
        private string newFolderName;
        private string status;
        private string lastOperation = "-";

        private static readonly string AVATAR_KEY = "email";
        private static readonly string NAME_KEY = "name";
        private static readonly string ACCESS_TOKEN_KEY = "accessToken";
    }
}
