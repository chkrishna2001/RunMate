using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RunMate.Models;
using RunMate.ViewModels;

namespace RunMate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {

            InitializeComponent();
            var vm = new MainViewModel(DialogCoordinator.Instance);
            DataContext = vm;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<WindowLoadedMessage>();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeNode selectedNode && selectedNode.IsCommand)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.SelectedCommand = selectedNode;
                    vm.Command = selectedNode.CommandText;
                    vm.SelectedShell = selectedNode.ShellType; // (optional if you want to auto-select Shell too)
                }
            }
        }
    }
}