namespace Prism_Drive;
using CommunityToolkit.Maui.Views;
using Prism_Drive.Models;
using Prism_Drive.Services;
using Prism_Drive.ViewModels;
using Prism_Drive.Views;

public partial class MainPage : ContentPage
{


    public MainPage()
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += MainViewModel_PropertyChanged;

    }

    private async void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.ShowLoginPopup))
        {
            if (_viewModel.ShowLoginPopup)
            {
                _viewModel.ShowLoginPopup = false;
                var popup = new Login(App.Current.Services.GetRequiredService<IUserService>());

                var userProxy = await this.ShowPopupAsync(popup);

                if (userProxy != null)
                {
                    _viewModel.PrismUser = userProxy as PrismUser;
                }
            }
        }
    }

    private MainViewModel? _viewModel;

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await _viewModel.Initialize();
    }
}

