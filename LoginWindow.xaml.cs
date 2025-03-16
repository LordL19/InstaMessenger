using System.Windows;

namespace InstantMessenger
{
    public partial class LoginWindow : Window
    {
        public string Username { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // Colocar el cursor en el TextBox al iniciar
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Permitir presionar Enter para iniciar sesión
            UsernameTextBox.KeyDown += (s, e) => {
                if (e.Key == System.Windows.Input.Key.Enter)
                    LoginButton_Click(s, e);
            };

            // Establecer nombre de usuario predeterminado (opcional)
            UsernameTextBox.Text = System.Environment.UserName;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Validar que se haya ingresado un nombre de usuario
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario válido.",
                                "Nombre requerido",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Guardar el nombre de usuario ingresado
            Username = UsernameTextBox.Text.Trim();

            // Crear y abrir la ventana principal
            MainWindow mainWindow = new MainWindow(Username);
            mainWindow.Show();

            this.Close();
        }
    }
}