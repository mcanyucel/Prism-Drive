using Prism_Drive.Models;

namespace Prism_Drive.Services
{
    internal interface IHttpService
    {
        Task<PrismUser> GetAccessTokenAsync(string email, string password, string token_name);
        Task<string> GetFileListsAsync(string accessToken);
    }
}
