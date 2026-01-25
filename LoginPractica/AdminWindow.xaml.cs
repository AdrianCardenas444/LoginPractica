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
        private DataTable dtUsuarios;
        private string adminNombre;

        public AdminWindow(string admin)
        {
            InitializeComponent();
            this.adminNombre = admin;
            CargarTodo();
        }

        private void CargarTodo()
        {
            dtUsuarios = db.ObtenerUsuarios();
            gridUsuarios.ItemsSource = dtUsuarios.DefaultView;
            ActualizarLogs();
        }

        // --- BUSCADOR DINÁMICO (Requisito 3) ---
        private void TxtBusquedaAdmin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dtUsuarios != null)
            {
                // Filtramos la tabla por el nombre de usuario [cite: 47-48]
                dtUsuarios.DefaultView.RowFilter = $"nombre_usuario LIKE '%{txtBusquedaAdmin.Text}%'";
            }
        }

        // --- MOSTRAR LOGS (Extensión) ---
        private void ActualizarLogs()
        {
            DataTable dtLogs = new DataTable();
            using (MySqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT admin_usuario as 'Admin', accion as 'Acción', usuario_afectado as 'Afectado', fecha as 'Fecha' FROM LogActividad ORDER BY fecha DESC";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.Fill(dtLogs);
                    gridLogs.ItemsSource = dtLogs.DefaultView;
                }
                catch { }
            }
        }

        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridUsuarios.SelectedItem is DataRowView fila)
            {
                txtId.Text = fila["id"].ToString();
                txtUser.Text = fila["nombre_usuario"].ToString();
                txtPass.Text = fila["password"].ToString();
                txtEmail.Text = fila["email"].ToString();
                cmbRol.Text = fila["rol"].ToString();
                chkBan.IsChecked = fila["estado"].ToString() == "0";
            }
        }

        // --- ACCIONES CRUD ---

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            int id = int.Parse(txtId.Text);
            int est = chkBan.IsChecked == true ? 0 : 1;

            if (db.EditarUsuario(id, txtPass.Text, txtEmail.Text, cmbRol.Text, est))
            {
                db.RegistrarLog(adminNombre, "EDITAR/BANEO", txtUser.Text); // [cite: 59-61]
                CargarTodo();
                MessageBox.Show("Usuario modificado y acción registrada.");
            }
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (db.AgregarUsuarioAdmin(txtUser.Text, txtPass.Text, txtEmail.Text, cmbRol.Text))
            {
                db.RegistrarLog(adminNombre, "CREAR", txtUser.Text);
                CargarTodo();
                MessageBox.Show("Nuevo usuario creado.");
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            if (MessageBox.Show("¿Eliminar usuario permanentemente?", "Baja", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.EliminarUsuario(int.Parse(txtId.Text));
                db.RegistrarLog(adminNombre, "ELIMINAR", txtUser.Text);
                CargarTodo();
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}