using CommunityToolkit.Maui.Views;
using Prism_Drive.Services;
using Prism_Drive.Services.Implementation;

namespace Prism_Drive.Views;

public partial class Login : Popup
{

    private readonly IUserService _userService;


    public Login(IUserService userService)
    {
        _userService = userService;
        InitializeComponent();
        
    }

    private async Task TryLoginAsync()
    {

        btnLogin.IsEnabled = false;
        var email = txtUsername.Text;
        var password = txtPassword.Text;

        txtLoginStatus.Text = "Acquiring user...";

        UserRequestResult userRequestResult = await _userService.GetUserAsync(email, password);

        if (userRequestResult.IsSuccess)
        {
            if (!chbSavePassword.IsToggled)
            {
                userRequestResult.PrismUser.Password = string.Empty;
            }
            Close(userRequestResult.PrismUser);
        }
        else
        {
            txtLoginStatus.Text = userRequestResult.Message;
        }
        btnLogin.IsEnabled = true;
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {
        await TryLoginAsync();
    }
}