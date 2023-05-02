using CommunityToolkit.Maui.Views;
using Prism_Drive.Services;

namespace Prism_Drive.Views;

public partial class Login : Popup
{

    private readonly IHttpService httpService;


    public Login()
    {
        InitializeComponent();
        httpService = App.Current.Services.GetService<IHttpService>();
    }

    private async Task TryLoginAsync()
    {

        btnLogin.IsEnabled = false;
        var email = txtUsername.Text;
        var password = txtPassword.Text;
        var token_name = "prism-drive";

        txtLoginStatus.Text = "Acquiring user...";

        var user = await httpService.GetAccessTokenAsync(email, password, token_name);

        if (user == null)
        {
            txtLoginStatus.Text = "Error getting user details!";
        }

        else
        {
            Close(user);
        }

        btnLogin.IsEnabled = true;
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        await TryLoginAsync();
    }
}