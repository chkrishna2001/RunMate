using CommunityToolkit.Mvvm.ComponentModel;

namespace RunMate.Models
{
    public partial class CommandEntry : ObservableObject
    {

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string commandText;

        [ObservableProperty]
        private string shellType;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private string originalName;
    }


}
