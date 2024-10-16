using CommunityToolkit.Mvvm.ComponentModel;

namespace Prism_Drive.Models
{
    public class UploadItem : ObservableObject
    {

        public FileResult FileResult { get; set; }
        public bool IsSelected { get; set; } = false;
        public string Status { get => status; set => SetProperty(ref status, value); }

        private string status = "Pending";
    }
}
