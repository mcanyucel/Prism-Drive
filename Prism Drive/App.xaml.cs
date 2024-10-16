
using Prism_Drive.Extensions;
using Prism_Drive.Services;
using Prism_Drive.Services.Implementation;
using Prism_Drive.ViewModels;

namespace Prism_Drive;

public partial class App : Application
{
	public const string PRISM_DRIVE_HTTP_CLIENT_NAME = "PrismDriveHttpClient";

    public new static App Current => (App)Application.Current;

    public App()
	{
		Services = ConfigureServices();

		InitializeComponent();
		MainPage = new AppShell();
		
    }

	public IServiceProvider Services { get; }

	private static ServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();

		services
			.AddCoreServices()
			.AddPrismServices()
			.AddUserServices()
            .AddViewModelMappings();

		return services.BuildServiceProvider();
	}
}
