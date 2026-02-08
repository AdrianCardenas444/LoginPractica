using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace LoginPractica
{
    public partial class AdminWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();
        private string adminName;

        public AdminWindow(string admin)
        {
            InitializeComponent();
            adminName = admin;
            RecargarTodo();
        }

        private void RecargarTodo()
        {
            // Usuarios (Importante: Asignamos DefaultView para poder filtrar)
            gridUsuarios.ItemsSource = db.ObtenerUsuarios().DefaultView;

            // Logs
            using (var conn = db.GetConnection())
            {
                conn.Open();
                DataTable dt = new DataTable();
                new MySqlDataAdapter("SELECT * FROM LogActividad ORDER BY fecha DESC", conn).Fill(dt);
                gridLogs.ItemsSource = dt.DefaultView;
            }
            // Juegos
            gridJuegos.ItemsSource = db.ObtenerJuegos();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        // --- BUSCADOR DE USUARIOS (NUEVO) ---
        private void TxtBusquedaUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Obtenemos la Vista de Datos de la tabla
                DataView dv = gridUsuarios.ItemsSource as DataView;
                if (dv != null)
                {
                    string filtro = txtBusquedaUser.Text.Trim();
                    // Filtramos por Nombre de Usuario O por Email
                    if (string.IsNullOrEmpty(filtro))
                    {
                        dv.RowFilter = ""; // Sin filtro
                    }
                    else
                    {
                        dv.RowFilter = $"nombre_usuario LIKE '%{filtro}%' OR email LIKE '%{filtro}%'";
                    }
                }
            }
            catch { }
        }

        // --- USUARIOS ---
        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridUsuarios.SelectedItem is DataRowView r)
            {
                txtId.Text = r["id"].ToString();
                txtUser.Text = r["nombre_usuario"].ToString();
                txtPass.Text = r["password"].ToString();
                txtEmail.Text = r["email"].ToString();
                cmbRol.Text = r["rol"].ToString();
                chkBan.IsChecked = r["estado"].ToString() == "0";
            }
        }
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            db.EditarUsuario(int.Parse(txtId.Text), txtPass.Text, txtEmail.Text, cmbRol.Text, chkBan.IsChecked == true ? 0 : 1);
            db.RegistrarLog(adminName, "EDITAR USER", txtUser.Text); RecargarTodo();
        }
        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (db.AgregarUsuarioAdmin(txtUser.Text, txtPass.Text, txtEmail.Text, cmbRol.Text))
            {
                db.RegistrarLog(adminName, "CREAR USER", txtUser.Text); RecargarTodo();
            }
        }
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtId.Text))
            {
                db.EliminarUsuario(int.Parse(txtId.Text));
                db.RegistrarLog(adminName, "ELIMINAR USER", txtUser.Text); RecargarTodo();
            }
        }

        // --- JUEGOS ---

        private void GridJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridJuegos.SelectedItem is Juego j)
            {
                txtJuegoId.Text = j.Id.ToString();
                txtJuegoTitulo.Text = j.Titulo;
                txtJuegoCat.Text = j.Categoria;
                txtJuegoPrecio.Text = j.Precio.ToString();
                txtJuegoImg.Text = j.ImagenUrl;
                txtJuegoDesc.Text = j.Descripcion;
            }
        }

        private void BtnEditJuego_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtJuegoId.Text)) return;
            try
            {
                int id = int.Parse(txtJuegoId.Text);
                decimal precio = decimal.Parse(txtJuegoPrecio.Text);

                if (db.EditarJuego(id, txtJuegoTitulo.Text, txtJuegoDesc.Text, precio, txtJuegoCat.Text, txtJuegoImg.Text))
                {
                    db.RegistrarLog(adminName, "EDITAR JUEGO", txtJuegoTitulo.Text);
                    RecargarTodo();
                    MessageBox.Show("Juego actualizado correctamente.");
                }
            }
            catch { MessageBox.Show("Error: Revisa que el precio sea numérico."); }
        }

        private void BtnAddJuego_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (db.AgregarJuego(txtJuegoTitulo.Text, txtJuegoDesc.Text, decimal.Parse(txtJuegoPrecio.Text), txtJuegoCat.Text, txtJuegoImg.Text))
                {
                    db.RegistrarLog(adminName, "ALTA JUEGO", txtJuegoTitulo.Text); RecargarTodo(); MessageBox.Show("Juego creado");
                }
            }
            catch { MessageBox.Show("Revisa los datos"); }
        }

        private void BtnDelJuego_Click(object sender, RoutedEventArgs e)
        {
            if (gridJuegos.SelectedItem is Juego j)
            {
                if (MessageBox.Show($"¿Borrar {j.Titulo}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (db.EliminarJuego(j.Id)) { db.RegistrarLog(adminName, "BAJA JUEGO", j.Titulo); RecargarTodo(); }
                }
            }
        }
    }
}