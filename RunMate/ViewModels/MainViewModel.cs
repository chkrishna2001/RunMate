using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using RunMate.Models;
using RunMate.Services;

namespace RunMate.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDialogCoordinator dialogCoordinator;
        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            SavedCommands = new ObservableCollection<TreeNode>();
            this.dialogCoordinator = dialogCoordinator;
            WeakReferenceMessenger.Default.Register<WindowLoadedMessage>(this, (data, msg) =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    var loaded = CommandRepositoryLiteDbService.LoadTree();
                    if (loaded != null)
                    {
                        AllCommands.Clear();
                        AllCommands.AddRange(loaded);
                        SavedCommands.Clear();
                        foreach (var cmd in loaded)
                            SavedCommands.Add(cmd);
                    }
                });
            });

        }


        [ObservableProperty]
        private string command;

        [ObservableProperty]
        private string result;

        [ObservableProperty]
        private bool isExecuting;

        [ObservableProperty]
        private TreeNode selectedCommand;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private ObservableCollection<TreeNode> savedCommands;
        [ObservableProperty]
        private string selectedShell = "PowerShell";

        private List<TreeNode> AllCommands { get; } = new();

        public ObservableCollection<string> AvailableShells { get; } =
            new() { "PowerShell", "pwsh", "cmd", "bash" };

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SavedCommands.Clear();
                foreach (var node in AllCommands)
                {
                    SavedCommands.Add(CloneNode(node)); // Deep copy to SavedCommands
                }
                CollapseAll(SavedCommands);
                return;
            }

            var filtered = new ObservableCollection<TreeNode>();

            foreach (var node in AllCommands)
            {
                var matchedNode = SearchAndClone(node, value);
                if (matchedNode != null)
                {
                    filtered.Add(matchedNode);
                }
            }

            SavedCommands.Clear();
            foreach (var item in filtered)
            {
                SavedCommands.Add(item);
            }
            ExpandAll(SavedCommands);
        }

        [RelayCommand]
        private void EnableRename(TreeNode entry)
        {
            if (entry == null)
                return;
            entry.IsRenaming = true;
            entry.OriginalName = entry.Name;
        }
        [RelayCommand]
        private void CancelRename(TreeNode entry)
        {
            if (entry == null)
                return;
            entry.IsRenaming = false;
            entry.Name = entry.OriginalName;
        }

        [RelayCommand]
        private async Task CommitRename(TreeNode entry)
        {
            if (entry == null)
                return;
            entry.IsRenaming = false;

            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                entry.Name = entry.OriginalName;
                await dialogCoordinator.ShowMessageAsync(this, "Error", "Name cannot be empty");
                return;
            }
            var parent = FindParentNode(SavedCommands, entry);
            bool isDuplicate = parent.Children.Any(c => c.Name.Equals(entry.OriginalName, StringComparison.OrdinalIgnoreCase) && c.IsCommand);
            if (isDuplicate)
            {
                entry.Name = entry.OriginalName;
                await dialogCoordinator.ShowMessageAsync(this, "Error", "Duplicate or empty name not allowed.");
                return;
            }

            await Task.Delay(300); // debounce effect
            SaveCommandTreeAndReload();
            await dialogCoordinator.ShowMessageAsync(this, "Success", "Command renamed successfully!", MessageDialogStyle.Affirmative);

        }


        [RelayCommand]
        private async Task DeleteCommand(TreeNode entry)
        {
            if (entry == null) return;

            var result = await dialogCoordinator.ShowMessageAsync(this,
                "Delete Confirmation",
                $"Are you sure you want to delete \"{entry.Name}\"?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                SavedCommands.Remove(entry);
                SaveCommandTreeAndReload();
                await dialogCoordinator.ShowMessageAsync(this, "Deleted", $"\"{entry.Name}\" has been deleted.", MessageDialogStyle.Affirmative);
            }
        }


        [RelayCommand]
        private async Task Execute()
        {
            IsExecuting = true;
            try
            {
                Result = "Running.....";
                await Task.Delay(1);
                Result = await CommandExecutorService.Execute(Command, SelectedShell);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        [RelayCommand]
        private void AddNew()
        {
            //var newCmd = new CommandEntry { Name = "New Command", CommandText = "", ShellType = "PowerShell" };
            //SavedCommands.Add(newCmd);
            //SelectedCommand = newCmd;
        }



        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                return;
            }
            if (SelectedCommand != null)
            {
                var nodeToUpdate = FindNodeById(SavedCommands, SelectedCommand.Id);
                if (nodeToUpdate != null)
                {
                    nodeToUpdate.CommandText = Command;
                    nodeToUpdate.ShellType = SelectedShell;
                }
            }
            else
            {
                var newCommandNode = new TreeNode
                {
                    CommandText = Command,
                    IsCommand = true,
                    ShellType = SelectedShell,
                    Name = "New Command"
                };
                var selectedShellNode = SavedCommands.FirstOrDefault(m => m.ShellType == SelectedShell);
                if (selectedShellNode != null)
                {
                    selectedShellNode.Children.Add(newCommandNode);
                }
                else
                {
                    SavedCommands.Add(new TreeNode
                    {
                        Name = SelectedShell,
                        ShellType = SelectedShell,
                        CommandText = null,
                        IsCommand = false,
                        Children = new ObservableCollection<TreeNode>([newCommandNode])
                    });
                }
            }
            SaveCommandTreeAndReload();
        }
        [RelayCommand]
        private void Copy()
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                return;
            }
            Clipboard.SetText(Result);
        }
        [RelayCommand]
        private void SaveToFile()
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                return;
            }
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(Path.Combine(tempFilePath), Result);
            Process.Start(new ProcessStartInfo { FileName = tempFilePath, UseShellExecute = true });
        }

        [RelayCommand]
        private void AddGroup(TreeNode parent)
        {
            var newGroup = new TreeNode
            {
                Name = "New Group",
                IsCommand = false
            };
            parent.Children.Add(newGroup);
        }

        [RelayCommand]
        private void AddCmd(TreeNode parent)
        {
            var newCommand = new TreeNode
            {
                Name = "New Command",
                IsCommand = true,
                CommandText = "Enter command here..."
            };
            parent.Children.Add(newCommand);
        }
        [RelayCommand]
        private void MoveUp(TreeNode treeNode)
        {
            if (treeNode == null) return;

            var parent = FindParentNode(SavedCommands, treeNode);
            var siblings = parent != null ? parent.Children : SavedCommands;

            var index = siblings.IndexOf(treeNode);
            if (index > 0)
            {
                siblings.Move(index, index - 1);
            }

            SaveCommandTreeAndReload();
        }
        [RelayCommand]
        private void MoveDown(TreeNode treeNode)
        {
            if (treeNode == null) return;

            var parent = FindParentNode(SavedCommands, treeNode);
            var siblings = parent != null ? parent.Children : SavedCommands;

            var index = siblings.IndexOf(treeNode);
            if (index < siblings.Count - 1)
            {
                siblings.Move(index, index + 1);
            }

            SaveCommandTreeAndReload();
        }


        [RelayCommand]
        private void DeleteNode(TreeNode node)
        {
            if (node == null) return;

            // Find parent node recursively
            TreeNode parent = FindParentNode(SavedCommands, node);
            if (parent != null)
            {
                parent.Children.Remove(node);
            }
            else
            {
                SavedCommands.Remove(node); // maybe deleting a root node
            }
            SaveCommandTreeAndReload();
        }
        private TreeNode FindParentNode(ObservableCollection<TreeNode> nodes, TreeNode child)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Contains(child))
                    return node;

                var found = FindParentNode(node.Children, child);
                if (found != null)
                    return found;
            }
            return null;
        }
        private bool DeleteNodeById(ObservableCollection<TreeNode> nodes, Guid idToDelete)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.Id == idToDelete)
                {
                    nodes.RemoveAt(i);
                    return true; // ✅ Deleted
                }

                // Recursively search in children
                if (node.Children != null && DeleteNodeById(node.Children, idToDelete))
                {
                    return true; // ✅ Deleted inside child
                }
            }

            return false; // ❌ Not found
        }
        private TreeNode FindNodeById(ObservableCollection<TreeNode> nodes, Guid id)
        {
            foreach (var node in nodes)
            {
                if (node.Id == id)
                    return node;

                if (node.Children?.Count > 0)
                {
                    var found = FindNodeById(node.Children, id);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
        private void SaveCommandTreeAndReload()
        {
            CommandRepositoryLiteDbService.SaveTree(SavedCommands);
            AllCommands.Clear();
            AllCommands.AddRange(CommandRepositoryLiteDbService.LoadTree());
        }
        private TreeNode CloneNode(TreeNode node)
        {
            return new TreeNode
            {
                Name = node.Name,
                IsCommand = node.IsCommand,
                CommandText = node.CommandText,
                ShellType = node.ShellType,
                Children = new ObservableCollection<TreeNode>(node.Children?.Select(CloneNode) ?? Enumerable.Empty<TreeNode>())
            };
        }
        private TreeNode SearchAndClone(TreeNode node, string searchText)
        {
            bool isMatch = node.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            var matchingChildren = node.Children?
                .Select(child => SearchAndClone(child, searchText))
                .Where(child => child != null)
                .ToList();

            if (isMatch || (matchingChildren?.Any() == true))
            {
                return new TreeNode
                {
                    Name = node.Name,
                    IsCommand = node.IsCommand,
                    CommandText = node.CommandText,
                    ShellType = node.ShellType,
                    Children = new ObservableCollection<TreeNode>(matchingChildren ?? Enumerable.Empty<TreeNode>())
                };
            }

            return null; // Not matching, and no matching children
        }
        private void ExpandAll(ObservableCollection<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsExpanded = true;
                if (node.Children?.Count > 0)
                {
                    ExpandAll(node.Children);
                }
            }
        }

        private void CollapseAll(ObservableCollection<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsExpanded = false;
                if (node.Children?.Count > 0)
                {
                    CollapseAll(node.Children);
                }
            }
        }


    }
}
