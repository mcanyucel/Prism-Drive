using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prism_Drive.Views;

public partial class Login : Popup
{
    private static readonly HttpClient _httpClient = new();


    public Login()
    {
        InitializeComponent();
        // since this is a Popup, it cannot be databound.
    }

    private async Task TryLoginAsync()
    {
        var email = txtUsername.Text;
        var password = txtPassword.Text;
        var token_name = txtTokenName.Text;

        var jsonString = JsonSerializer.Serialize(new { email, password, token_name });

        using StringContent jsonContent = new(
            jsonString,
            Encoding.UTF8,
            "application/json"
            );

        txtLoginStatus.Text = "Acquiring access token...";

        using HttpResponseMessage httpResponse = await _httpClient.PostAsync("https://app.prismdrive.com/api/v1/auth/login", jsonContent);

        if (httpResponse.IsSuccessStatusCode)
        {
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            

            Regex regex = AccessTokenRegex();
            var accessTokenMatch = regex.Match(jsonResponse);

            if (accessTokenMatch.Success)
            {
                var accessToken = accessTokenMatch.ToString().Split(':')[1].Replace("\"", String.Empty);
                txtLoginStatus.Text = "Access token acquired, saving the token...";
                Debug.WriteLine(accessToken);
            }
            else
            {
                txtLoginStatus.Text = "Cannot extract access token, invalid response!";
            }
        }
        else
        {
            txtLoginStatus.Text = $"Server connection error: {httpResponse.StatusCode.ToString()}";
        }
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        await TryLoginAsync();
    }

    [GeneratedRegex("\"access_token\":\"(?:[^\"]|\"\")*\"", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AccessTokenRegex();
}