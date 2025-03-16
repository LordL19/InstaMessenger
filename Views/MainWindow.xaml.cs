using InstantMessenger.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace InstantMessenger.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(string username)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(username);
            DataContext = _viewModel;

            Title = $"Instant Messenger - {username}";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Limpiar recursos
            _viewModel.Cleanup();
        }
    }
}