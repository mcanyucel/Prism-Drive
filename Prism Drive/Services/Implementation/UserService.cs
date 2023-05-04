using Prism_Drive.Models;

namespace Prism_Drive.Services.Implementation
{
    class UserService : IUserService
    {
        public UserService(IPrismService prismServiceProxy)
        {
            prismService = prismServiceProxy;
        }

        public async Task<UserRequestResult> GetUserAsync()
        {
            var email = Preferences.Default.Get(EMAIL_KEY, string.Empty);
            var password = Preferences.Default.Get(PASSWORD_KEY, string.Empty);

            PrismUser user = null;
            string message = string.Empty;
            bool isSuccess = false;

            if (email != string.Empty && password != string.Empty)
            {
                try
                {
                    user = await prismService.FetchUserAsync(email, password);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            // if no credentials are found locally, return null with a result of Success so the UI can display the login popup
            else
            {
                isSuccess = true;
                message = "No credentials found";
            }
            return new UserRequestResult
            {
                PrismUser = user,
                Message = message,
                IsSuccess = isSuccess
            };
        }

        public async Task<UserRequestResult> GetUserAsync(string email, string password)
        {
            PrismUser user = null;
            string message = string.Empty;
            bool isSuccess = false;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                message = "Email and password are required";
            }
            else
            {
                try
                {
                    user = await prismService.FetchUserAsync(email, password);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }

            return new UserRequestResult
            {
                PrismUser = user,
                IsSuccess = isSuccess,
                Message = message
            };
        }

        public bool SaveUser(PrismUser user)
        {
            bool success = true;

            try
            {
                Preferences.Default.Set(EMAIL_KEY, user.Email);
                Preferences.Default.Set(PASSWORD_KEY, user.Password);
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool RemoveUser()
        {
            bool success = true;
            try
            {
                Preferences.Default.Remove(EMAIL_KEY);
                Preferences.Default.Remove(PASSWORD_KEY);
            }

            catch
            {
                success = false;
            }

            return success;
        }

        private readonly IPrismService prismService;

        private static readonly string EMAIL_KEY = "email";
        private static readonly string PASSWORD_KEY = "password";

    }
}
