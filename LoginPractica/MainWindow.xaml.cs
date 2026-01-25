using System.Windows;
using System.Windows.Media;

namespace LoginPractica
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public MainWindow() => InitializeComponent();

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string pass = txtPassword.Password;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(pass)) { lblMensaje.Text = "Rellena todos los campos"; return; }

            string rol = dbHelper.ValidarLogin(usuario, pass); 

            if (rol == "BANEADO") { lblMensaje.Text = "ACCESO DENEGADO: Usuario baneado."; return; } 
            if (rol != null) 
            {
                new HomeWindow(usuario, rol).Show();
                this.Close();
            } 
            else { lblMensaje.Text = "Usuario o contraseña incorrectos."; }
        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Password;
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(email)) 
            { 
                lblMensaje.Text = "Para registrarte necesitas Usuario, Pass y Email."; 
                return; 
            }

            if (dbHelper.AgregarUsuarioAdmin(user, pass, email, "user")) 
            {
                lblMensaje.Foreground = Brushes.LightGreen;
                lblMensaje.Text = "¡Registro exitoso! Ya puedes loguearte.";
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MostrarError(string m) { lblMensaje.Foreground = Brushes.Red; lblMensaje.Text = m; }
    }
}