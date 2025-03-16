using InstantMessenger.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace InstantMessenger.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public ICommand LoginCommand { get; }

        public event EventHandler<string> LoginSuccessful;

        public LoginViewModel()
        {
            // Establecer nombre de usuario predeterminado
            Username = Environment.UserName;

            // Definir el comando de inicio de sesión
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username);
        }

        private void ExecuteLogin(object parameter)
        {
            // Validar la entrada
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario válido.",
                               "Nombre requerido",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Notificar que el login fue exitoso
            LoginSuccessful?.Invoke(this, Username.Trim());
        }
    }
}