using Prism_Drive.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prism_Drive.Services.Implementation
{
    internal partial class PrismService : IPrismService
    {

        public async Task<UploadResult> UploadFile(UploadItem uploadItem, string uploadDirectory, string accessToken)
        {

            

            var content = new MultipartFormDataContent
                {
                    { new StringContent("null"), "parent_id" },
                    { new StringContent(uploadItem.FileResult.FileName), "name" },
                    { new StringContent(uploadItem.FileResult.ContentType), "content_type" },
                    { new StreamContent(await uploadItem.FileResult.OpenReadAsync()), "file", uploadItem.FileResult.FileName }
                };

            if (!string.IsNullOrWhiteSpace(uploadDirectory))
            {
                var relativePath = uploadDirectory + uploadItem.FileResult.FileName;
                content.Add(new StringContent(relativePath), "relativePath");
            }

            var result = new UploadResult();

            using HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uploadEndpoint),
                Headers =
                {
                    { "Authorization", "Bearer " + accessToken }
                },
                Content = content

            };

            using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            result.IsSuccess = httpResponse.IsSuccessStatusCode;
            result.Message = await httpResponse.Content.ReadAsStringAsync();

            return result;
        }

        public async Task CreateFolderAsync(string folderPath, string accessToken)
        {
            var payloadString = "{\"name\": \"" + folderPath + "\",  \"parentId\": null }";
            using StringContent payload = new(payloadString, Encoding.UTF8, "application/json");

            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(createFolderEndpoint),
                Headers =
                {
                    { "Authorization", "Bearer " + accessToken }
                },
                Content = new StringContent(payloadString)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                }
            };

            using HttpResponseMessage httpResponse = await httpClient.SendAsync(request);

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task<PrismUser> FetchUserAsync(string email, string password)
        {
            var jsonString = JsonSerializer.Serialize(new { email, password, token_name });
            using StringContent jsonContent = new(jsonString, Encoding.UTF8, "application/json");
            using HttpResponseMessage httpResponse = await httpClient.PostAsync(loginEndpoint, jsonContent);

            httpResponse.EnsureSuccessStatusCode();

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
                    Email = email,
                    Password = password
                };
            }
            else
            {
                throw new InvalidDataException("The login response is invalid.");
            }
        }

        public async Task<string> GetFileListsAsync(string accessToken)
        {
            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(fileListsEndpoint),
                Headers =
                {
                    { "Authorization", "Bearer " + accessToken }
                }
            };

            using HttpResponseMessage httpResponse = await httpClient.SendAsync(request);


            httpResponse.EnsureSuccessStatusCode();
            var body = await httpResponse.Content.ReadAsStringAsync();
            return body;
        }

        [GeneratedRegex("\"access_token\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AccessTokenRegex();

        [GeneratedRegex("\"display_name\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex DisplayNameRegex();

        [GeneratedRegex("\"avatar\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AvatarUrlRegex();

        private static readonly HttpClient httpClient = new()
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        private static readonly string loginEndpoint = "https://app.prismdrive.com/api/v1/auth/login";
        private static readonly string fileListsEndpoint = "https://app.prismdrive.com/api/v1/file-entries?perPage=50";
        private static readonly string createFolderEndpoint = "https://app.prismdrive.com/api/v1/folders";
        private static readonly string uploadEndpoint = "https://app.prismdrive.com/api/v1/uploads";

        private static readonly string token_name = "PrismDrive";

        private static readonly string EMAIL_KEY = "email";
        private static readonly string PASSWORD_KEY = "password";
    }
}
