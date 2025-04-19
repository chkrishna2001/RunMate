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
    }
}