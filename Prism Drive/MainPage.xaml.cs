namespace Prism_Drive;
using CommunityToolkit.Maui.Views;
using Prism_Drive.Views;

public partial class MainPage : ContentPage
{

	private string accessToken = null;

	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnCounterClicked(object sender, EventArgs e)
	{
		var popup = new Login();
		var result = await this.ShowPopupAsync(popup);
	}
}

