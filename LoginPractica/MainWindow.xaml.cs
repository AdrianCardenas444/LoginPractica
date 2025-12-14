using System.Windows;
using System.Windows.Media;

namespace LoginPractica
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();
        private int intentosFallidos = 0;
        private string ultimoUsuario = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- LOGIN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblMensaje.Text = "";
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Introduce usuario y contraseña.");
                return;
            }

            if (usuario == ultimoUsuario && intentosFallidos >= 3)
            {
                MostrarError("Usuario bloqueado temporalmente.");
                return;
            }

            if (dbHelper.ValidarLogin(usuario, password))
            {
                intentosFallidos = 0;

                // --- CAMBIO AQUÍ: Pasamos el usuario a la nueva ventana ---
                HomeWindow home = new HomeWindow(usuario);
                home.Show();
                this.Close();
            }
            else
            {
                if (usuario != ultimoUsuario) { intentosFallidos = 0; ultimoUsuario = usuario; }
                intentosFallidos++;
                MostrarError($"Credenciales incorrectas. Intento {intentosFallidos} de 3.");
            }
        }

        // --- REGISTRO ---
        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Para crear cuenta, rellena los datos.");
                return;
            }

            if (dbHelper.RegistrarUsuario(usuario, password))
            {
                lblMensaje.Foreground = Brushes.LightGreen;
                lblMensaje.Text = "¡Cuenta creada! Inicia sesión.";
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76));
            lblMensaje.Text = mensaje;
        }
    }
}