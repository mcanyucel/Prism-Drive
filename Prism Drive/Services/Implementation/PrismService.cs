﻿using Prism_Drive.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prism_Drive.Services.Implementation
{
    internal partial class PrismService : IPrismService
    {

        public async Task<bool> CreateFolderAsync(string folderPath, string accessToken)
        {
            var payloadString = "{\"name\": \"" + folderPath + "\",  \"parentId\": null }";
            using StringContent payload = new(payloadString, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using HttpResponseMessage httpResponse = await httpClient.PostAsync(createFolderEndpoint, payload);
            if (httpResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using HttpResponseMessage httpResponse = await httpClient.GetAsync(fileListsEndpoint);

            if (httpResponse.IsSuccessStatusCode)
            {
                return "I get a value back!";
            }
            else
            {
                switch (httpResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.Forbidden:
                        return "You don't have required permissions for this action.";
                    case System.Net.HttpStatusCode.Unauthorized:
                        return "Invalid access token";
                    default:
                        return "Error";
                }
            }

        }

        [GeneratedRegex("\"access_token\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AccessTokenRegex();

        [GeneratedRegex("\"display_name\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex DisplayNameRegex();

        [GeneratedRegex("\"avatar\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
        private partial Regex AvatarUrlRegex();

        private static readonly HttpClient httpClient = new();
        private static readonly string loginEndpoint = "https://app.prismdrive.com/api/v1/auth/login";
        private static readonly string fileListsEndpoint = "https://app.prismdrive.com/api/v1/file-entries?perPage=50";
        private static readonly string createFolderEndpoint = "https://app.prismdrive.com/api/v1/folders";

        private static readonly string token_name = "PrismDrive";

        private static readonly string EMAIL_KEY = "email";
        private static readonly string PASSWORD_KEY = "password";
    }
}