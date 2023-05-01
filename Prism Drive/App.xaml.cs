
using Prism_Drive.Services;
using Prism_Drive.Services.Implementation;

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

		services.AddSingleton<IHttpService, HttpService>();

		return services.BuildServiceProvider();
	}

}
