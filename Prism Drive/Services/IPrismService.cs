using Prism_Drive.Models;

namespace Prism_Drive.Services
{
    internal interface IPrismService
    {
        Task<PrismUser> FetchUserAsync(string email, string password);
        Task<string> GetFileListsAsync(string accessToken);

        Task CreateFolderAsync(string folderName, string accessToken);
    }
}
