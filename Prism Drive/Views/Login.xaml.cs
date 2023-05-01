using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prism_Drive.Views;

public partial class Login : Popup
{
    private static HttpClient _httpClient = new();


    public Login()
    {
        InitializeComponent();
    }

    private async Task TryLoginAsync()
    {
        var email = "mcanyucel@proton.me";
        var password = "arekKa56";
        var token_name = "maui";

        var jsonString = JsonSerializer.Serialize(new { email, password, token_name });

        using StringContent jsonContent = new(
            jsonString,
            Encoding.UTF8,
            "application/json"
            );

        using HttpResponseMessage httpResponse = await _httpClient.PostAsync("https://app.prismdrive.com/api/v1/auth/login", jsonContent);

        if (httpResponse.IsSuccessStatusCode)
        {
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            

            Regex regex = AccessTokenRegex();
            var accessTokenMatch = regex.Match(jsonResponse);

            if (accessTokenMatch.Success)
            {
                var accessToken = accessTokenMatch.ToString().Split(':')[1].Replace("\"", String.Empty);
                Debug.WriteLine(accessToken);
            }
            else
            {
                Debug.WriteLine("Invalid response");
            }
        }
        else
        {
            Debug.Write(httpResponse.StatusCode);
        }
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        await TryLoginAsync();
    }

    [GeneratedRegex("\"access_token\":\"(?:[^\"]|\"\")*\"", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AccessTokenRegex();
}