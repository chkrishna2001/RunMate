using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RunMate.Models
{
    public partial class TreeNode : ObservableObject
    {
        [ObservableProperty]
        private Guid id;
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isCommand; // true = it's a command; false = it's a group

        [ObservableProperty]
        private string commandText; // command body if it's a command

        [ObservableProperty]
        private string shellType; // available at root node or inherited
        [ObservableProperty]
        private bool isRenaming;
        [ObservableProperty]
        private bool isExpanded;
        [ObservableProperty]
        private string originalName;
        [ObservableProperty]
        private bool isSelected;
        public ObservableCollection<TreeNode> Children { get; set; } = new();
    }

}
