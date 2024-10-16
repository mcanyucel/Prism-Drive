using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
                AsyncCommandCanExecuteChanged(CreateFolderCommand, SelectFilesCommand, UploadFilesCommand); ;
            }
        }
        public PrismUser PrismUser
        {
            get => prismUser;
            set
            {
                SetProperty(ref prismUser, value);
                Status = value == null ? "Not logged in" : "Ready";
                if (value != null) { userService.SaveUser(value); }
                CommandCanExecuteChanged(LoginCommand, LogoutCommand, RemoveSelectedCommand);
                AsyncCommandCanExecuteChanged(CreateFolderCommand, SelectFilesCommand, UploadFilesCommand);
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

        public async Task Initialize()
        {
            await CheckUser();

            if (PrismUser != null)
            {
                var res = await PrismService.GetFileListsAsync(PrismUser.AccessToken);
            }
        }

        private async Task GetFileList()
        {
            try
            {
                var res = await PrismService.GetFileListsAsync(PrismUser.AccessToken);
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        public MainViewModel(IPrismService httpServiceProxy, IUserService userServiceProxy)
        {
            PrismService = httpServiceProxy;
            userService = userServiceProxy;

            LoginCommand = new RelayCommand(Login, LoginCanExecute);
            LogoutCommand = new RelayCommand(Logout, LogoutCanExecute);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected, RemoveSelectedCanExecute);
            CreateFolderCommand = new AsyncRelayCommand(CreateFolder, CreateFolderCanExecute);
            SelectFilesCommand = new AsyncRelayCommand(SelectFiles, SelectFilesCanExecute);
            UploadFilesCommand = new AsyncRelayCommand(UploadFiles, UploadFilesCanExecute);

            SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;

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
            IsBusy = true;
            Status = "Uploading files...";

            await Parallel.ForEachAsync(SelectedFiles, async (file, uploadCancellationToken) =>
            {
                file.Status = "Uploading";
                var result = await PrismService.UploadFile(file, UploadDirectoryPath, PrismUser.AccessToken);
                file.Status = result.IsSuccess ? "Uploaded" : "Failed";
                LastOperation = $"{file.FileResult.FileName}: {file.Status}. ({DateTime.Now})";
            });

            Status = "Ready";
            IsBusy = false;
        }

        private bool UploadFilesCanExecute()
        {
            return PrismUser != null && IsBusy == false && SelectedFiles.Count > 0;
        }

        private async Task SelectFiles()
        {
            await PrismService.GetFileListsAsync(PrismUser.AccessToken);
            //IsBusy = true;
            //try
            //{
            //    var result = await FilePicker.PickMultipleAsync();
            //    if (result != null)
            //    {
            //        foreach (var file in result)
            //        {
            //            if (!SelectedFiles.Any(x => x.FileResult.FullPath == file.FullPath))
            //            {
            //                SelectedFiles.Add(new UploadItem { FileResult = file });
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"{ex.Message}", ex);
            //}
            //IsBusy = false;
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
            try
            {
                await PrismService.CreateFolderAsync(NewFolderName, PrismUser.AccessToken);
                LastOperation = $"Created {NewFolderName}. ({DateTime.Now})";
            }
            catch(Exception ex)
            {
                LastOperation = $"Failed to create {NewFolderName}. ({DateTime.Now})";
                Status = ex.Message;
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
            userService.RemoveUser();
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

        private async Task CheckUser()
        {
            IsBusy = true;
            Status = "Acquiring user access token...";

            var userRequestResult = await userService.GetUserAsync();
            if (userRequestResult.IsSuccess)
            {
                PrismUser = userRequestResult.PrismUser;
            }
            else
            {
                Status = userRequestResult.Message;
            }

            IsBusy = false;
        }

        private bool isBusy = false;
        private readonly IPrismService PrismService;
        private readonly IUserService userService;
        private PrismUser prismUser;
        private bool showLoginPopup;
        private string newFolderName;
        private string status = "Not logged in";
        private string lastOperation = "-";
        private string uploadDirectoryPath;
        private CancellationToken uploadCancellationToken = new CancellationTokenSource().Token;
    }
}
