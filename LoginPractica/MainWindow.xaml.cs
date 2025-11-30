using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LoginPractica
{
    public partial class MainWindow : Window
    {
        // 1. Base de datos simulada
        private Dictionary<string, string> baseDeDatosUsuarios = new Dictionary<string, string>()
        {
            { "admin", "1234" },
            { "gamer", "steam" },
            { "profesor", "clase" }
        };

        // 2. Contador de intentos fallidos
        private Dictionary<string, int> intentosFallidos = new Dictionary<string, int>();

        public MainWindow()
        {
            InitializeComponent();

            // Inicializamos contadores a 0
            foreach (var user in baseDeDatosUsuarios.Keys)
            {
                intentosFallidos.Add(user, 0);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Limpiamos mensaje anterior al pulsar el botón
            lblMensaje.Text = "";

            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            // CASO 3: Campos vacíos
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Introduzca datos");
                return;
            }

            // CASO 2: El usuario no existe
            if (!baseDeDatosUsuarios.ContainsKey(usuario))
            {
                MostrarError("El usuario introducido no existe");
                return;
            }

            // CASO 6 (Bloqueo previo): Verificar si ya estaba bloqueado
            if (intentosFallidos[usuario] >= 3)
            {
                MostrarError($"El usuario '{usuario}' está bloqueado temporalmente.");
                return;
            }

            // VERIFICACIÓN DE CONTRASEÑA
            if (baseDeDatosUsuarios[usuario] == password)
            {
                // ÉXITO
                intentosFallidos[usuario] = 0; // Resetear intentos

                // Mensaje opcional en verde antes de cambiar
                lblMensaje.Foreground = Brushes.LightGreen;
                lblMensaje.Text = "Acceso correcto...";

                // Navegación
                HomeWindow home = new HomeWindow();
                home.Show();
                this.Close();
            }
            else
            {
                // CASO 1 y 6: Contraseña mal y contador
                intentosFallidos[usuario]++;
                int fallos = intentosFallidos[usuario];

                if (fallos >= 3)
                {
                    MostrarError("Usuario Bloqueado por seguridad.");
                }
                else
                {
                    // Solo dice que la contraseña está mal
                    MostrarError($"Contraseña incorrecta. Intento {fallos} de 3.");
                }
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Método auxiliar para escribir en rojo en la ventana
        private void MostrarError(string mensaje)
        {
            lblMensaje.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76)); // Rojo Steam
            lblMensaje.Text = mensaje;
        }
    }
}