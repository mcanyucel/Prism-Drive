using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Prism_Drive.Models;
using Prism_Drive.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Prism_Drive.ViewModels
{
    internal class MainViewModel : ObservableObject
    {




        public bool IsBusy { get => isBusy; set
            {
                SetProperty(ref isBusy, value);
                CommandCanExecuteChanged(LoginCommand, LogoutCommand, RemoveSelectedCommand);
                AsyncCommandCanExecuteChanged(CreateFolderCommand, SelectFilesCommand); ;
                Status = IsBusy ? "Working..." : "Ready";
            }
        }
        public PrismUser PrismUser
        {
            get => prismUser;
            set
            {
                SetProperty(ref prismUser, value);
                SaveUser();
                CommandCanExecuteChanged(LoginCommand, LogoutCommand, RemoveSelectedCommand);
                AsyncCommandCanExecuteChanged(CreateFolderCommand, SelectFilesCommand);
            }
        }
        public IRelayCommand LoginCommand { get; set; }
        public IRelayCommand LogoutCommand { get; set; }
        public IRelayCommand RemoveSelectedCommand { get; set; }
        public IAsyncRelayCommand CreateFolderCommand { get; set; }
        public IAsyncRelayCommand SelectFilesCommand { get; set; }
        public IAsyncRelayCommand UploadFilesCommand { get; set; }
        public bool ShowLoginPopup { get => showLoginPopup; set => SetProperty(ref showLoginPopup, value); }
        public string NewFolderName { get => newFolderName; set { SetProperty(ref newFolderName, value); AsyncCommandCanExecuteChanged(CreateFolderCommand); } }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public string LastOperation { get => lastOperation; set => SetProperty(ref lastOperation, value); }

        public ObservableCollection<UploadItem> SelectedFiles { get; } = new ObservableCollection<UploadItem>();

        public string UploadDirectoryPath 
        { 
            get => uploadDirectoryPath; 
            set 
            {
                SetProperty(ref uploadDirectoryPath, value); AsyncCommandCanExecuteChanged(UploadFilesCommand);
            } 
        }

        public async Task GetFileList()
        {
            var fileList = await httpService.GetFileListsAsync(PrismUser.AccessToken);
            Debug.WriteLine($"\n\n\n{fileList}\n\n\n");
        }

        public MainViewModel(IHttpService httpServiceProxy)
        {
            httpService = httpServiceProxy;

            LoginCommand = new RelayCommand(Login, LoginCanExecute);
            LogoutCommand = new RelayCommand(Logout, LogoutCanExecute);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected, RemoveSelectedCanExecute);
            CreateFolderCommand = new AsyncRelayCommand(CreateFolder, CreateFolderCanExecute);
            SelectFilesCommand = new AsyncRelayCommand(SelectFiles, SelectFilesCanExecute);
            UploadFilesCommand = new AsyncRelayCommand(UploadFiles, UploadFilesCanExecute);
            
            CheckUser();

            SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;

            Status = "Ready";
        }

        private void RemoveSelected()
        {
            SelectedFiles.Where(q => q.IsSelected).ToList().ForEach(q => SelectedFiles.Remove(q));
        }

        private bool RemoveSelectedCanExecute()
        {
            return PrismUser != null && IsBusy == false && SelectedFiles.Count > 0;
        }

        private void SelectedFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AsyncCommandCanExecuteChanged(UploadFilesCommand);
            CommandCanExecuteChanged(RemoveSelectedCommand);
        }

        private async Task UploadFiles()
        {
            await Task.Run(()=> Debug.WriteLine(UploadDirectoryPath));
        }

        private bool UploadFilesCanExecute()
        {
            return PrismUser != null && IsBusy == false && SelectedFiles.Count > 0;
        }

        private async Task SelectFiles()
        {
            IsBusy = true;
            try
            {
                var result = await FilePicker.PickMultipleAsync();
                if (result != null)
                {
                    foreach (var file in result)
                    {
                        if (!SelectedFiles.Any(x => x.FileResult.FullPath == file.FullPath))
                        {
                            SelectedFiles.Add(new UploadItem { FileResult = file });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}", ex);
            }
            IsBusy = false;
        }

        private bool SelectFilesCanExecute()
        {
            return PrismUser != null && IsBusy == false;
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

        private bool isBusy = false;
        private readonly IHttpService httpService;
        private PrismUser prismUser;
        private bool showLoginPopup;
        private string newFolderName;
        private string status;
        private string lastOperation = "-";
        private string uploadDirectoryPath;


        private static readonly string AVATAR_KEY = "email";
        private static readonly string NAME_KEY = "name";
        private static readonly string ACCESS_TOKEN_KEY = "accessToken";
    }
}
