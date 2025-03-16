using InstantMessenger.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace InstantMessenger.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public string Username => _viewModel.Username;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Suscribirnos al evento de login exitoso
            _viewModel.LoginSuccessful += OnLoginSuccessful;

            // Permitir presionar Enter para iniciar sesión
            KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                    _viewModel.LoginCommand.Execute(null);
            };

            // Colocar el cursor en el TextBox al iniciar
            Loaded += (s, e) => {
                var textBoxes = LogicalTreeHelper.FindLogicalNode(this, "UsernameTextBox");
                (textBoxes as UIElement)?.Focus();
            };
        }

        private void OnLoginSuccessful(object sender, string username)
        {
            // Indicar que el login fue exitoso y cerrar la ventana
            DialogResult = true;
            Close();
        }
    }
}