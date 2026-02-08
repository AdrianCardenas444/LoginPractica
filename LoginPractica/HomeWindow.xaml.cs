using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LoginPractica
{
    public partial class HomeWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private List<Juego> catalogoCompleto;
        private List<int> idsMisJuegos;
        private List<Juego> carrito = new List<Juego>();
        private string usuarioActual;

        public HomeWindow(string user, string rol)
        {
            InitializeComponent();
            usuarioActual = user;
            txtUserName.Text = user.ToUpper();

            if (rol == "admin") btnAdmin.Visibility = Visibility.Visible;

            IniciarReloj();
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            // Limpiamos listas por si es una recarga
            carrito.Clear();
            ActualizarVistaCarrito();

            // Descargamos datos frescos de la BBDD
            catalogoCompleto = db.ObtenerJuegos();
            idsMisJuegos = db.ObtenerIdsMisJuegos(usuarioActual);

            // Pintamos la tienda
            RenderizarTienda(catalogoCompleto);
        }

        // --- AQUÍ ESTÁ EL CAMBIO PARA QUE SE ACTUALICE LA TIENDA ---
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow admin = new AdminWindow(usuarioActual);
            admin.ShowDialog(); // El código se pausa aquí hasta que cierras el admin

            // Al volver, recargamos todo para ver los juegos nuevos
            CargarDatosIniciales();
        }

        // --- EL RESTO SIGUE IGUAL ---
        private void RenderizarTienda(List<Juego> juegos)
        {
            PanelTienda.Children.Clear();
            foreach (var j in juegos)
            {
                Border card = CrearTarjetaBase(j);
                StackPanel sp = (StackPanel)card.Child;

                Button btn = new Button { Margin = new Thickness(10), Padding = new Thickness(5), FontWeight = FontWeights.Bold, Tag = j };

                if (idsMisJuegos.Contains(j.Id))
                {
                    btn.Content = "EN BIBLIOTECA"; btn.Background = Brushes.Gray; btn.IsEnabled = false;
                }
                else if (carrito.Exists(x => x.Id == j.Id))
                {
                    btn.Content = "EN EL CARRO"; btn.Background = Brushes.Orange; btn.IsEnabled = false;
                }
                else
                {
                    btn.Content = $"{j.Precio}€ | AÑADIR"; btn.Background = new SolidColorBrush(Color.FromRgb(92, 126, 16));
                    btn.Foreground = Brushes.White; btn.Cursor = System.Windows.Input.Cursors.Hand;
                    btn.Click += (s, e) => AgregarAlCarrito(j);
                }
                sp.Children.Add(btn);
                PanelTienda.Children.Add(card);
            }
        }

        private void RenderizarBiblioteca()
        {
            PanelBiblioteca.Children.Clear();
            var misJuegos = catalogoCompleto.Where(j => idsMisJuegos.Contains(j.Id)).ToList();
            foreach (var j in misJuegos)
            {
                Border card = CrearTarjetaBase(j);
                StackPanel sp = (StackPanel)card.Child;
                Button btn = new Button
                {
                    Content = "JUGAR",
                    Background = new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                    Foreground = Brushes.White,
                    Margin = new Thickness(10),
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };
                btn.Click += (s, e) => MessageBox.Show($"Iniciando {j.Titulo}...");
                sp.Children.Add(btn);
                PanelBiblioteca.Children.Add(card);
            }
        }

        private void AgregarAlCarrito(Juego j)
        {
            carrito.Add(j);
            ActualizarVistaCarrito();
            RenderizarTienda(catalogoCompleto);
        }

        private void ActualizarVistaCarrito()
        {
            btnNavCarro.Content = $"CARRITO ({carrito.Count})";
            gridCarrito.ItemsSource = null; gridCarrito.ItemsSource = carrito;
            txtTotalCarro.Text = $"{carrito.Sum(x => x.Precio)}€";
        }

        private void BtnConfirmarCompra_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0) return;
            if (db.ConfirmarCompraCarrito(usuarioActual, carrito))
            {
                MessageBox.Show("¡Compra realizada!");
                foreach (var j in carrito) idsMisJuegos.Add(j.Id);
                carrito.Clear();
                ActualizarVistaCarrito();
                Nav_Click(btnNavBiblio, null);
            }
            else MessageBox.Show("Error al comprar.");
        }

        private void BtnVaciar_Click(object sender, RoutedEventArgs e) { carrito.Clear(); ActualizarVistaCarrito(); RenderizarTienda(catalogoCompleto); }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            VistaTienda.Visibility = Visibility.Collapsed;
            VistaBiblioteca.Visibility = Visibility.Collapsed;
            VistaCarrito.Visibility = Visibility.Collapsed;
            btnNavTienda.Foreground = Brushes.LightGray; btnNavBiblio.Foreground = Brushes.LightGray;

            if (tag == "Tienda") { VistaTienda.Visibility = Visibility.Visible; btnNavTienda.Foreground = new SolidColorBrush(Color.FromRgb(102, 192, 244)); RenderizarTienda(catalogoCompleto); }
            else if (tag == "Biblioteca") { VistaBiblioteca.Visibility = Visibility.Visible; btnNavBiblio.Foreground = new SolidColorBrush(Color.FromRgb(102, 192, 244)); RenderizarBiblioteca(); }
            else if (tag == "Carrito") { VistaCarrito.Visibility = Visibility.Visible; }
        }

        private Border CrearTarjetaBase(Juego j)
        {
            Border card = new Border
            {
                Width = 200,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(35, 60, 81)),
                CornerRadius = new CornerRadius(5),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            card.MouseLeftButtonDown += (s, e) => new GameDetailsWindow(j).ShowDialog();

            StackPanel sp = new StackPanel();
            Image img = new Image { Height = 260, Stretch = Stretch.UniformToFill, Margin = new Thickness(0, 0, 0, 5) };
            img.Style = (Style)FindResource("ZoomImageStyle");
            try { img.Source = new BitmapImage(new Uri(j.ImagenUrl)); } catch { }

            TextBlock title = new TextBlock { Text = j.Titulo, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(10, 5, 10, 0), TextTrimming = TextTrimming.CharacterEllipsis };
            sp.Children.Add(img); sp.Children.Add(title);
            card.Child = sp;
            return card;
        }

        private void TxtBuscador_TextChanged(object sender, TextChangedEventArgs e) { RenderizarTienda(catalogoCompleto.FindAll(x => x.Titulo.ToLower().Contains(txtBuscador.Text.ToLower()))); }
        private void IniciarReloj() { DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) }; t.Tick += (s, e) => lblReloj.Text = DateTime.Now.ToString("HH:mm"); t.Start(); }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
        private void BtnSalir_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}