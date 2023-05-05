# Prism Drive

A multi-platform program to upload files to the Prism Drive. It has been tested on Windows and Android.

## Installation

Clone the repository and build the project with Visual Studio. Note that Cross-Platform development workload should be installed to support MAUI platforms (Android, iOS, macOS, Windows).

## Usage

### Creating folders

A folder can be created using the "Create Folder" button. The folder will always be created in the root directory. Note that typing a path as the folder name does not work; a folder with the name of the path will be created in the root directory instead.

### Uploading files

Due to the API issues (see below), the program cannot fetch the file list from the Prism Drive. Therefore, the user has to manually enter the file name and the folder name (if the file is not in the root directory) to upload a file. The file will be uploaded to the root directory if the folder name is not specified. Currently the program supports specifying a single upload folder; all the files will be uploaded to this same folder.

To remove a file from the upload list, select the file and click the "Remove Selected" button. The "Upload All" button will upload all the files in the upload list (selected and unselected).

## Issues - Limitations

* The program cannot fetch the file list from the Prism Drive. The API seems to be broken and always returns 403 - Forbidden. Even the official Prism Drive API sample script cannot fetch the file list. This prevents listing the remote files or downloading them. However, the program can still upload files to the Prism Drive.

* The program cannot upload files larger than 10 GB. This is due to the Prism Drive API limitation.

* File name collisions are not handled. If a file with the same name already exists in the upload folder, the file be uploaded with the same name, and there will be two files with the same name in the upload folder. The Prism API does not support versioning.

* The upload progress is not shown. The Prism API does not support this.

* The upload speed is extremely slow; uploading of a 2 kB file takes about 6-7 minutes. This is possibly due to the Prism API throttling the uploads. It also seems that the Prism API groups simultaneous uploads to a single upload speed limit. For example, if there are 10 files in the upload list, the upload speed is the same as if there was only one file in the upload list. These are only guesses, as the Prism API documentation does not mention anything about throttling or upload speed limits.

* The program does not support uploading folders. The Prism API does not support this.

* The program does not support adding all files in a folder to the upload list. The MAUI Community API does not support this (yet): user can select a folder, but there is no direct way to traverse all files in that folder. There is no platform independent way to do this yet, therefore it is skipped for now.

## Additional Dependencies

* `CommunityToolkit.Maui`: The .NET MAUI Community Toolkit is a collection of Animations, Behaviors, Converters, and Custom Views for development with .NET MAUI. It simplifies and demonstrates common developer tasks building iOS, Android, macOS and Windows apps with .NET MAUI.
* `CommunityToolkit.Mvvm`: This package includes a .NET MVVM library with helpers.
* `Microsoft.Extensions.DependencyInjection`: Default implementation of dependency injection for Microsoft.Extensions.DependencyInjection.

## Logging In Details

The Prism API requires that access token should be sent along with every request to PrismDrive.com API in authorization header: `Authorization: Bearer <Token>`. This token is acquired via `auth/login` endpoint on every time the application is started, since the tokens seem to have a short lifetime. 

### `auth/login` endpoint

**NOTE**: The information on the [official documentation](https://app.prismdrive.com/api-docs) is incorrect. Instead of the payload given there, the following payload should be sent:
```json
{
  "email": "<email>",
  "password": "<password>",
  "token_name": "<token_name>"
}
```

If a token with `token_name` does not exist, it will be created and returned. If a token with `token_name` already exists, it will be returned. The token name can be anything, but it should be unique. The token name is used to identify the token when deleting it.

The token name for the application is saved in the private static readonly string `token_name` in the `PrismService` class (See `Services`).

The response returned is a JSON object that includes all the user details. This application uses regular expressions to parse the response and extract the token with the user details, instead of deserializing the response to a class.

### Saving user information

Since the token has a relatively short lifetime, it is not saved and should be acquired every session (every time the application is started). If the user chooses to save their credentials, the email and password are saved using the default Microsoft implementation `Preferences` of `IPreferences` interface. 

**NOTE**: The password is saved as plain text. This is not secure, but Prism API does not support hashing the password (See future plans). For details on saving user details, see `Services`. 

If the user does not choose to save their credentials, the email is still recorded but the password is saved as an empty string. This is done to prevent the user from having to type their email every time the application is started. However, the email is not automatically typed to the email field yet (see future plans).

## License

MIT License. See `LICENSE.txt` for more information.

# Future Plans

* The UI is not yet optimized for mobile devices. The UI should be redesigned to be more mobile-friendly.
* The Prism API does not support hashing the password. The password is saved as plain text. This is not secure. The password should be encrypted before saving it to the device, then decrypted when the user logs in. 
* Several functions are directly called from the UI code-behind callbacks (e.g. `ContentPage_Loaded`), which does not conform to MVVM practice. This is to take a shortcut on the events that do not support direct command binding. These should be moved to a command binding, using the Interactivity package (or its equivalent for MAUI).
* Stopping the upload process is not supported. A `CancellationToken` should be used to support this.
* Uploading multiple files concurrently is achieved by `Parallel.ForEachAsync` without a `CancellationToken` (There is actually a `CancellationToken` instance supplied to the `ParallelOptions`, but it is not used). Since every loop is an asynchronous operation running on parallel, it is not canceled when the application is closed. This causes the ongoing uploads to continue even after the application is closed. A `CancellationToken` should be used to cancel the ongoing uploads when the application is closed.
* A way to add all files in a folder to the upload list should be implemented. This is not supported by the MAUI Community API yet (see `Issues - Limitations`).

# Detailed Documentation

## BindingConverters

This namespace contains the `IValueConverter` implementations used in the application for UI databinding.

### `InverseUserBooleanConverter`
Returns true if the user is null (not logged in), false otherwise. This is used to show the login page if the user is not logged in, and the main page if the user is logged in.
This is a one-way converter and it will throw `InvalidOperationException` if the `ConvertBack` method is called (i.e. trying to convert a `bool` to a user).

### `ItemStatusBooleanConverter`
Returns true if the item status is `Uploading`, false otherwise. This is used to show the upload activity indicatior if the item status is `Uploading`.
This is a one-way converter and it will throw `InvalidOperationException` if the `ConvertBack` method is called (i.e. trying to convert a `bool` to a status string).

### `ItemStatusColorConverter`
Returns a color based on the given `Status` string. This is used to show the file status text in different colors based on the status.
This is a one-way converter and it will throw `InvalidOperationException` if the `ConvertBack` method is called (i.e. trying to convert a `Color` to a status string).

### `UserBooleanConverter`
Returns true if the user is not null (logged in), false otherwise. This is used to show the login page if the user is not logged in, and the main page if the user is logged in.
This is a one-way converter and it will throw `InvalidOperationException` if the `ConvertBack` method is called (i.e. trying to convert a `bool` to a user).

## Models

This namespace contains the model classes used in the application.

### `PrismUser`
This class represents a Prism user. It contains the user details returned by the Prism API when the user logs in, including the access token. Since user details are not expected to change during a session, this class is not observable.

### `UploadItem`
This class represents an item to be uploaded. It contains the file details, upload status and if it is selected in the UI. This class is observable, since the upload status and progress are expected to change during a session.

## Services

This namespace contains the service classes used in the application. The service classes are responsible for all the business logic in the application. The view-model should only call the service methods, and onlt the service methods should call Prism API, file system, and other methods. The service classes should not be aware of the UI or the view-model, and the view-model should not be aware of the concrete implementation of services. This is achieved by using interfaces for the services and dependency injection (specifically constructor injection); all service classes registered as singletons in the `App` service configuration class, therefore there is only a single instance of each service class in the application at a time.

### `IPrismService` and `PrismService`
This interface defines the methods that are implemented by the `PrismService` class. 

* `Task<PrismUser> FetchUserAsync(string email, string password)`: This method asynchoronously fetches the user details from the Prism API using the given email and password. If the user details are successfully fetched, the user is logged in and the user details are returned. If the user details cannot be fetched, an `InvalidDataException` is thrown. The response is a json object that includes the user details and the access token. The access token and other details are extracted using regular expressions and saved in the `PrismUser` instance. The `PrismUser` instance is then returned.

* `Task<string> GetFileListsAsync(string accessToken)`: This method asynchoronously fetches the file lists from the Prism API using the given access token. If the file lists are successfully fetched, the file lists are returned as a json string. If the file lists cannot be fetched, an `InvalidDataException` is thrown. The response is a json object that includes the file lists. The file lists are extracted using regular expressions and returned as a json string. **NOTE:** This method is currently broken as the Prism API does not return the file lists in the response. This method is not used in the application.

* `Task CreateFolderAsync(string folderName, string accessToken);`: This method asynchoronously creates a folder with the given name in the Prism API using the given access token. If the folder cannot be created, an exception is thrown.

* `Task<UploadResult> UploadFile(UploadItem uploadItem, string uploadDirectory, string accessToken)`: This method asynchronously uploads the given file to the Prism API using the given access token. It returns an instance of `UploadResult`, which contains the details of the upload. 

### `IUserService` and `UserService`

This interface defines the methods that are implemented by the `UserService` class.

* `Task<UserRequestResult> GetUserAsync()`: This method asynchronously gets the user credentials from the file system (using `IPreferences` API). If the credentials are found, then it uses the `IPrismService` to fetch the user details from the Prism API. It returns a `UserRequestResult` instance, which contains the details of the request. If the credentials are not found, then the `UserRequestResult` instance contains the error message.

* `Task<UserRequestResult> GetUserAsync(string email, string password);`: This method asynchronously uses the `IPrismService` to fetch the user details from the Prism API using the given email and password. It returns a `UserRequestResult` instance, which contains the details of the request. If the credentials are not found, then the `UserRequestResult` instance contains the error message.

* `bool SaveUser(PrismUser user);`: This method saves the user credentials to the file system (using `IPreferences` API). It returns true if the credentials are successfully saved, false otherwise.

* `bool RemoveUser()`: This method removes the user credentials from the file system (using `IPreferences` API). It returns true if the credentials are successfully removed, false otherwise.

### `UploadResult`

This class represents the result of an upload operation. If the `IsSuccess` flag is false, the `Message` property contains the error message. If the `IsSuccess` flag is true, the `Message` property contains the upload result message.

### `UserRequestResult`

This class represents the result of a user request operation. If the `IsSuccess` flag is false, the `Message` property contains the error message. If the `IsSuccess` flag is true, the `Message` property contains the user request result message. If the `IsSuccess` flag is true, the `PrismUser` property contains the user details.

## ViewModels

This namespace contains the view-model classes used in the application. The view-model classes are responsible for the UI logic in the application. The view-model classes should only call the service methods, and only the service methods should call Prism API, file system, and other methods. The service classes should not be aware of the UI or the view-model, and the view-model should not be aware of the concrete implementation of services. This is achieved by using interfaces for the services and dependency injection (specifically constructor injection); all service classes registered as singletons in the `App` service configuration class, therefore there is only a single instance of each service class in the application at a time.

### `MainViewModel`

This class is the view-model for the main page. It implements the `ObservableObject` class since it is used as the `BindingContext` for the main page. It contains the following properties:

* `IsBusy`: This property is used to show the activity indicator when the application is busy. It is bound to the `IsBusy` property of the main page. It is also used to disable the UI controls when the application is busy over the `CanExecute` methods of the individual `ICommand` implementations.
* `PrismUser`: This property is used to show the user details in the UI. It is bound to the `PrismUser` property of the main page. It is also used to disable the UI controls when the user is not logged in over the `CanExecute` methods of the individual `ICommand` implementations.
* `LoginCommand`: This command is used to login the user. It is bound to the `Login` method of the main page.
* `LogoutCommand`: This command is used to logout the user. It is bound to the `Logout` method of the main page.
* `RemoveSelectedCommand`: This command is used to remove the selected files from the upload list. It is bound to the `RemoveSelected` method of the main page.
* `CreateFolderCommand`: This asynchronous command is used to create a folder in the Prism API. It is bound to the `CreateFolder` method of the main page.
* `SelectFilesCommand`: This asynchronous command is used to select files to be uploaded. It is bound to the `SelectFiles` method of the main page.
* `UploadFilesCommand`: This asynchronous command is used to upload the selected files to the Prism API. It is bound to the `UploadFiles` method of the main page.
* `ShowLoginPopup`: This property is used to show the login popup. It is observed in the `MainPage` class and the login popup is shown when this property is set to true. In theory, this functionality should be moved to a separate service class, but it is not done due to time constraints.
* `NewFolderName`: This property is used to get the name of the new folder to be created. It is bound to the `NewFolderName` property of the main page.
* `Status`: This property is used to show the status of the application. It is bound to the `Status` property of the main page.
* `LastOperation`: This property is used to show the last operation performed by the application. It is bound to the `LastOperation` property of the main page.
* `SelectedFiles`: This observable collection is used to show the selected files in the UI. It is initialized in the constructor of the `MainViewModel` class, and it is readonly.
* `UploadDirectoryPath`: This property is used to get the upload directory path from the UI. It is bound to the `UploadDirectoryPath` property of the main page.

The class also contains the following methods:

* `public async Task Initialize()`: This method is used to initialize the view-model. It is called from the `ContentPage_Loaded` callback of the main page (see `Limitations` and `Future Works`) . It calls the `CheckUser` method. If the Prism API is fixed (see `Limitations`), it should also call the `GetFileLists` method.
* `private async Task GetFileList()`: This method is used to get the file list from the Prism API. It calls the `GetFileListAsync` method of the `IPrismService` interface. If the Prism API is fixed (see `Limitations`), it should be called inside the `Initialize()` method.
* `private void RemoveSelected()`: This method is used to remove the selected files from the upload list. It is called from the `RemoveSelectedCommand` command.
* `private bool RemoveSelectedCanExecute()`: This method is used to determine if the `RemoveSelected` method can be executed. It is called from the `RemoveSelectedCommand` command.
* `private void SelectedFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)`: This method is used to update the `CanExecute` calls of the commands (that depend `SelectedFiles` collection) when the `SelectedFiles` collection is changed. It is subscribed to the `CollectionChanged` event of the `SelectedFiles` collection in the constructor of the `MainViewModel` class. This is required because `IRelayCommand` interface does not periodically check the `CanExecute` methods of the commands.
* `private async Task UploadFiles()`: This method is used to upload the selected files (asynchronously and concurrently) to the Prism API. It is called from the `UploadFilesCommand` command.
* `private bool UploadFilesCanExecute()`: This method is used to determine if the `UploadFiles` method can be executed. It is called from the `UploadFilesCommand` command.
* `private async Task SelectFiles()`: This method is used to select files to be uploaded. It is called from the `SelectFilesCommand` command.
* `private bool SelectFilesCanExecute()`: This method is used to determine if the `SelectFiles` method can be executed. It is called from the `SelectFilesCommand` command.
* `private async Task CreateFolder()`: This method is used to create a folder in the Prism API. It is called from the `CreateFolderCommand` command.
* `private bool CreateFolderCanExecute()`: This method is used to determine if the `CreateFolder` method can be executed. It is called from the `CreateFolderCommand` command.
* `private static void CommandCanExecuteChanged(params IRelayCommand[] commands)`: This method is used to call the `NotifyCanExecuteChanged` event of the given commands so that the `CanExecute` methods of the commands are called. It is a static method since it does not depend on the state of the view-model. This is necessary because `IRelayCommand` interface does not periodically check the `CanExecute` methods of the commands.
* `private static void AsyncCommandCanExecuteChanged(params IAsyncRelayCommand[] commands)`: This method is used to call the `NotifyCanExecuteChanged` event of the given asynchronous commands so that the `CanExecute` methods of the commands are called. It is a static method since it does not depend on the state of the view-model. This is necessary because `IAsyncRelayCommand` interface does not periodically check the `CanExecute` methods of the commands.
* `private void Logout()`: This method is used to logout the user. It is called from the `LogoutCommand` command.
* `private bool LogoutCanExecute()`: This method is used to determine if the `Logout` method can be executed. It is called from the `LogoutCommand` command.
* `private void Login()`: This method is used to login the user. It is called from the `LoginCommand` command.
* `private bool LoginCanExecute()`: This method is used to determine if the `Login` method can be executed. It is called from the `LoginCommand` command.
* `private async Task CheckUser()`: This method is used to check if the user is logged in. It is called from the `Initialize` method.

This class also contains the following private fields, excluding the backing fields of the properties defined above:

* `private CancellationToken uploadCancellationToken`: This field is used to cancel the upload operation. It is currently not used, but it should be used to cancel the upload operation when the application shuts down or the user wants to cancel the process (see `Future Works`).
* `private readonly IPrismService PrismService`: This field is used to access the Prism API. It is initialized in the constructor of the `MainViewModel` class via constructor injection.
* `private readonly IUserService userService;`: This field is used to access the user service. It is initialized in the constructor of the `MainViewModel` class via constructor injection.

## Views

### LoginView
It is the view of the login popup. It asks the user for credentials, and uses `IUserService` to login the user. It does not have a view-model since it is a popup, and it does not use data binding.

### MainPage
It is the main view of the application. It uses `MainViewModel` as its view-model. 