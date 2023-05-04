namespace Prism_Drive;
using CommunityToolkit.Maui.Views;
using Prism_Drive.Models;
using Prism_Drive.ViewModels;
using Prism_Drive.Views;
using System.Diagnostics;

public partial class MainPage : ContentPage
{


	public MainPage()
	{
		InitializeComponent();

		

		this.BindingContext = App.Current.Services.GetService<MainViewModel>();

        MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
	}

    private async void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.ShowLoginPopup)) {
			if (MainViewModel.ShowLoginPopup)
			{
                MainViewModel.ShowLoginPopup = false;
                var popup = new Login();

                var userProxy = await this.ShowPopupAsync(popup);

                if (userProxy != null)
                {
                    MainViewModel.PrismUser = userProxy as PrismUser;
                }
            }
		}
    }

    private MainViewModel MainViewModel => this.BindingContext as MainViewModel;

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await MainViewModel.Initialize();
    }
}

