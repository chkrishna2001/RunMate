using System.Collections.ObjectModel;
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
            SavedCommands = new ObservableCollection<CommandEntry>();
            this.dialogCoordinator = dialogCoordinator;
            WeakReferenceMessenger.Default.Register<WindowLoadedMessage>(this, (data, msg) =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    var loaded = CommandRepositoryService.LoadCommands();
                    if (loaded != null)
                    {
                        SavedCommands.Clear(); // 🔑 Important: clear instead of replacing
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
        private CommandEntry selectedCommand;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private ObservableCollection<CommandEntry> savedCommands;
        partial void OnSearchTextChanged(string value)
        {
            var commands = CommandRepositoryService.LoadCommands();
            var filteredCommands = commands.Where(m => m.Name.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();
            SavedCommands.Clear();
            foreach (var filteredCommand in filteredCommands)
            {
                SavedCommands.Add(filteredCommand);
            }

        }
        partial void OnSelectedCommandChanged(CommandEntry value)
        {
            Command = value.CommandText;
        }

        [RelayCommand]
        private void EnableRename(CommandEntry entry)
        {
            foreach (var item in SavedCommands)
            {
                item.IsEditing = false;
            }

            entry.OriginalName = entry.Name; // store original
            entry.IsEditing = true;
        }
        [RelayCommand]
        private void CancelRename(CommandEntry entry)
        {
            if (entry == null)
                return;
            entry.Name = entry.OriginalName;
            entry.IsEditing = false;
        }

        [RelayCommand]
        private async Task CommitRename(CommandEntry entry)
        {
            if (entry == null)
                return;
            entry.IsEditing = false;

            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                entry.Name = entry.OriginalName;
                await dialogCoordinator.ShowMessageAsync(this, "Error", "Name cannot be empty");
                return;
            }

            bool isDuplicate = SavedCommands.Any(c => c != entry && c.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
            if (isDuplicate)
            {
                entry.Name = entry.OriginalName;
                await dialogCoordinator.ShowMessageAsync(this, "Error", "Duplicate or empty name not allowed.");

                return;
            }

            await Task.Delay(300); // debounce effect
            CommandRepositoryService.SaveCommands(SavedCommands);
            await dialogCoordinator.ShowMessageAsync(this, "Success", "Command renamed successfully!", MessageDialogStyle.Affirmative);

        }


        [RelayCommand]
        private async Task DeleteCommand(CommandEntry entry)
        {
            if (entry == null) return;

            var result = await dialogCoordinator.ShowMessageAsync(this,
                "Delete Confirmation",
                $"Are you sure you want to delete \"{entry.Name}\"?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                SavedCommands.Remove(entry);
                CommandRepositoryService.SaveCommands(SavedCommands);
                await dialogCoordinator.ShowMessageAsync(this, "Deleted", $"\"{entry.Name}\" has been deleted.", MessageDialogStyle.Affirmative);
            }
        }


        [RelayCommand]
        private async Task Execute()
        {
            IsExecuting = true;
            try
            {
                var shellType = SelectedCommand?.ShellType ?? "PowerShell";
                Result = await Task.Run(() => CommandExecutorService.Execute(Command, shellType));
            }
            finally
            {
                IsExecuting = false;
            }
        }

        [RelayCommand]
        private void AddNewCommand()
        {
            var newCmd = new CommandEntry { Name = "New Command", CommandText = "", ShellType = "PowerShell" };
            SavedCommands.Add(newCmd);
            SelectedCommand = newCmd;
        }



        [RelayCommand]
        private void SaveCommand()
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                return;
            }
            if (SelectedCommand != null)
            {
                SelectedCommand.CommandText = Command;
                CommandRepositoryService.SaveCommands(SavedCommands);
            }
            else
            {
                var newCmd = new CommandEntry { Name = "New Command", CommandText = "", ShellType = "PowerShell" };
                SavedCommands.Add(newCmd);
                SelectedCommand = newCmd;
            }
        }
    }
}
