namespace Prism_Drive;
using CommunityToolkit.Maui.Views;
using Prism_Drive.Views;
using System.Diagnostics;

public partial class MainPage : ContentPage
{


	public MainPage()
	{
		InitializeComponent();
	}

    private async void CounterBtn_Clicked(object sender, EventArgs e)
    {
		var popup = new Login();

		var res = await this.ShowPopupAsync(popup);

		Debug.WriteLine(res);	
    }
}

