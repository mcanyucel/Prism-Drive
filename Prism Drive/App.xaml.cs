﻿
using Prism_Drive.Services;
using Prism_Drive.Services.Implementation;
using Prism_Drive.ViewModels;

namespace Prism_Drive;

public partial class App : Application
{
	public App()
	{
		Services = ConfigureServices();

		InitializeComponent();

		MainPage = new AppShell();
	}

	public new static App Current => (App)Application.Current;

	public IServiceProvider Services { get; }

	private static IServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();

		services.AddSingleton<IPrismService, PrismService>();
		services.AddSingleton<IUserService, UserService>();

		services.AddTransient<MainViewModel>();

		return services.BuildServiceProvider();
	}

}
