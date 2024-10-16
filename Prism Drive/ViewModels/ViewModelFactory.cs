using Prism_Drive.Extensions;
using Prism_Drive.Services;

namespace Prism_Drive.ViewModels;

public class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public TViewModel Create<TViewModel>() where TViewModel : IViewModel
    {
        int hash;
        TViewModel viewModel;
        hash = ServiceExtensions.GetHash(typeof(TViewModel).Name);
        viewModel = ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider, hash);

        return viewModel;
    }
}
