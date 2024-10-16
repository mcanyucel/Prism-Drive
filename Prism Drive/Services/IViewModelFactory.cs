using Prism_Drive.ViewModels;

namespace Prism_Drive.Services;

public interface IViewModelFactory
{
    TViewModel Create<TViewModel>() where TViewModel : IViewModel;
}
