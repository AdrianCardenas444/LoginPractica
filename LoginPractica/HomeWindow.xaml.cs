using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LoginPractica
{
    public partial class HomeWindow : Window
    {
        private DispatcherTimer reloj;

        public HomeWindow(string nombreUsuario)
        {
            InitializeComponent();
            txtUsuarioActivo.Text = nombreUsuario.ToUpper();
            IniciarReloj();
        }

        private void IniciarReloj()
        {
            reloj = new DispatcherTimer();
            reloj.Interval = TimeSpan.FromSeconds(1);
            reloj.Tick += Reloj_Tick;
            reloj.Start();
        }

        private void Reloj_Tick(object sender, EventArgs e)
        {
            lblReloj.Text = DateTime.Now.ToString("HH:mm");
        }

        // --- BUSCADOR (La clave para recolocar) ---
        private void TxtBuscador_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscador.Text.ToLower();
            int contadorVisibles = 0;

            foreach (var hijo in GamesPanel.Children)
            {
                if (hijo is Border tarjeta && tarjeta.Child is StackPanel panelInterior)
                {
                    // Título es el segundo elemento
                    if (panelInterior.Children.Count > 1 && panelInterior.Children[1] is TextBlock tituloJuego)
                    {
                        string nombreJuego = tituloJuego.Text.ToLower();

                        if (nombreJuego.Contains(filtro))
                        {
                            // IMPORTANTE: Visible hace que ocupe espacio
                            tarjeta.Visibility = Visibility.Visible;
                            contadorVisibles++;
                        }
                        else
                        {
                            // IMPORTANTE: Collapsed hace que DESAPAREZCA el hueco y los demás se muevan
                            tarjeta.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }

            // Actualizamos el contador de texto
            if (txtContador != null)
                txtContador.Text = $"Mostrando {contadorVisibles} juegos";
        }

        // --- FUNCIONALIDAD BOTÓN JUGAR (NUEVO) ---
        private void BtnJugar_Click(object sender, RoutedEventArgs e)
        {
            // Truco para saber qué botón se ha pulsado y pillar el nombre del juego
            Button botonPulsado = sender as Button;
            StackPanel panelPadre = botonPulsado.Parent as StackPanel;
            TextBlock tituloJuego = panelPadre.Children[1] as TextBlock; // El título es el segundo elemento

            string juego = tituloJuego.Text;

            MessageBox.Show($"Iniciando {juego}...\n\nPor favor, espere mientras conectamos con el servidor.",
                            "Lanzando Juego",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void BtnSalirApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
