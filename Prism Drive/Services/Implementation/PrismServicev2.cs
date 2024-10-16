using Prism_Drive.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prism_Drive.Services.Implementation;

public partial class PrismServicev2(IHttpClientFactory _clientFactory) : IPrismService, IDisposable
{
    public Task CreateFolderAsync(string folderName, string accessToken)
    {
        throw new NotImplementedException();
    }

    public async Task<PrismUser> FetchUserAsync(string email, string password)
    {
        var httpClient = _clientFactory.CreateClient("PRISM_CLIENT");
        var loginEndpoint = $"{BASE_URL}{LOGIN_ENDPOINT}";
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
        try
        {
            string endpoint = $"{BASE_URL}{FILE_LIST_ENDPOINT}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var client = _clientFactory.CreateClient("PRISM_CLIENT");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;

        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public Task<UploadResult> UploadFile(UploadItem uploadItem, string uploadDirectory, string accessToken)
    {
        throw new NotImplementedException();
    }

    [GeneratedRegex("\"access_token\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
    private partial Regex AccessTokenRegex();

    [GeneratedRegex("\"display_name\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
    private partial Regex DisplayNameRegex();

    [GeneratedRegex("\"avatar\":\"[^\"]*\"", RegexOptions.IgnoreCase)]
    private partial Regex AvatarUrlRegex();

    private const string BASE_URL = "https://app.prismdrive.com/api/v1";
    private const string FILE_LIST_ENDPOINT = "/file-entries";
    private const string UPLOAD_ENDPOINT = "/uploads";
    private const string CREATE_FOLDER_ENDPOINT = "/folders";
    private const string LOGIN_ENDPOINT = "/auth/login";
    private bool disposedValue;
    private static readonly string token_name = "PrismDrive";

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PrismServicev2()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
