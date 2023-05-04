using Prism_Drive.Models;

namespace Prism_Drive.Services.Implementation
{
    class UserRequestResult
    {
        public PrismUser PrismUser { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
