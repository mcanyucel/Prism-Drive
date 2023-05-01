using Prism_Drive.Models;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prism_Drive.Services.Implementation
{
    internal partial class HttpService : IHttpService
    {
        private static readonly HttpClient httpClient = new();
        private static readonly string loginEndpoint = "https://app.prismdrive.com/api/v1/auth/login"; 

        public async Task<PrismUser?> GetAccessTokenAsync(string email, string password, string token_name)
        {
            var jsonString = JsonSerializer.Serialize(new { email, password, token_name });
            using StringContent jsonContent = new(jsonString, Encoding.UTF8, "application/json");
            using HttpResponseMessage httpResponse = await httpClient.PostAsync(loginEndpoint, jsonContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                var accessTokenMatch = AccessTokenRegex().Match(jsonResponse);
                var displayNameMatch = DisplayNameRegex().Match(jsonResponse);
                var avatarUrlMatch = AvatarUrlRegex().Match(jsonResponse);
                
                // Only access token is mandatory
                if (accessTokenMatch.Success)
                {
                    var accessToken = accessTokenMatch.ToString().Split(':')[1].Replace("\"", string.Empty);
                    var displayName = displayNameMatch.Success ? displayNameMatch.ToString().Split(':')[1].Replace("\"", string.Empty) : "No Name";
                    var avatarUrlPre = String.Join(':', avatarUrlMatch.Success ? avatarUrlMatch.ToString().Split(':').Skip(1).ToArray() : Array.Empty<string>());
                    var avatarUrl = avatarUrlPre != string.Empty ? avatarUrlPre.Replace("\"", string.Empty).Replace("\\/", "/") : string.Empty;
                    return new PrismUser
                    {
                        AccessToken = accessToken,
                        DisplayName = displayName,
                        AvatarUrl = avatarUrl,
                    };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        [GeneratedRegex("\"access_token\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AccessTokenRegex();

        [GeneratedRegex("\"display_name\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex DisplayNameRegex();

        [GeneratedRegex("\"avatar\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AvatarUrlRegex();
    }


}
