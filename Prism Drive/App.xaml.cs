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

		services.AddTransient<LoginViewModel>();

		return services.BuildServiceProvider();
	}

}
