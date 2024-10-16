using Prism_Drive.Models;
using Prism_Drive.Services.Implementation;

namespace Prism_Drive.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Checks the local storage for a saved user credentials, and if found, attempts to fetch the user data from the Prism API.
        /// </summary>
        /// <returns>The user request result</returns>
        Task<UserRequestResult> GetUserAsync();
        /// <summary>
        /// Gets the user data from the Prism API
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<UserRequestResult> GetUserAsync(string email, string password);

        bool SaveUser(PrismUser user);

        bool RemoveUser();

    }
}
