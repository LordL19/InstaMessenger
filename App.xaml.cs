using InstantMessenger.Views;
using System.Windows;

namespace InstantMessenger
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Mostrar la ventana de login primero
            LoginWindow loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog();

            // Si el login fue exitoso (usuario presionó "Entrar al Chat")
            if (loginResult == true)
            {
                // Obtener el nombre de usuario personalizado
                string customUsername = loginWindow.Username;

                // Crear la ventana principal y pasar el nombre de usuario
                MainWindow mainWindow = new MainWindow(customUsername);
                this.MainWindow = mainWindow; // Establece MainWindow en la aplicación
                mainWindow.Show();
            }
            else
            {
                // Si el usuario cerró la ventana de login sin iniciar sesión
                Shutdown();
            }
        }
    }
}