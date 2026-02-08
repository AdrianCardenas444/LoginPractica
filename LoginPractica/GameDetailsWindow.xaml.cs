using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LoginPractica
{
    public partial class GameDetailsWindow : Window
    {
        public GameDetailsWindow(Juego j)
        {
            InitializeComponent();
            lblTitulo.Text = j.Titulo.ToUpper();
            lblCategoria.Text = j.Categoria;
            lblDescripcion.Text = j.Descripcion;
            lblPrecio.Text = $"{j.Precio}€";
            try { imgCaratula.Source = new BitmapImage(new Uri(j.ImagenUrl)); } catch { }
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}