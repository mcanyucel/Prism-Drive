using Prism_Drive.Models;
using Prism_Drive.Services.Implementation;

namespace Prism_Drive.Services
{
    internal interface IPrismService
    {
        Task<PrismUser> FetchUserAsync(string email, string password);
        Task<string> GetFileListsAsync(string accessToken);

        Task<UploadResult> UploadFile(UploadItem uploadItem, string uploadDirectory, string accessToken);

        Task CreateFolderAsync(string folderName, string accessToken);
    }
}
