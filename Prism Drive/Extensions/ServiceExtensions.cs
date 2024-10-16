namespace Prism_Drive.Extensions;

public static class ServiceExtensions
{
    public static int GetHash(string viewModelName, int? id = null) =>
        id.HasValue ? HashCode.Combine(viewModelName, id.Value) : viewModelName.GetHashCode();
}
