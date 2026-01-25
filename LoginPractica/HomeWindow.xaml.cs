using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LoginPractica
{
    public partial class HomeWindow : Window
    {
        private DispatcherTimer reloj;

        public HomeWindow(string nombreUsuario, string rol)
        {
            InitializeComponent();
            txtUsuarioActivo.Text = nombreUsuario.ToUpper();

            // Opción B: Si el rol es Admin, se habilita el acceso al formulario [cite: 64]
            if (rol == "admin") { btnAdminPanel.Visibility = Visibility.Visible; }

            IniciarReloj();
        }

        private void IniciarReloj()
        {
            reloj = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            reloj.Tick += (s, e) => {
                if (lblReloj != null) lblReloj.Text = DateTime.Now.ToString("HH:mm");
            };
            reloj.Start();
        }

        private void TxtBuscador_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscador.Text.ToLower();
            foreach (Border tarjeta in GamesPanel.Children)
            {
                StackPanel sp = (StackPanel)tarjeta.Child;
                TextBlock tb = (TextBlock)sp.Children[1];
                tarjeta.Visibility = tb.Text.ToLower().Contains(filtro) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos el nombre que tenemos en el TextBlock del usuario activo
            AdminWindow adminWin = new AdminWindow(txtUsuarioActivo.Text);
            adminWin.ShowDialog();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // Lógica para cerrar la aplicación definitivamente
        private void BtnSalirApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();


    }
}