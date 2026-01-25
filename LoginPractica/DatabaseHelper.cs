using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;

namespace LoginPractica
{
    public class DatabaseHelper
    {
        private string connectionString = "Server=localhost;Database=PracticaLoginDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection() => new MySqlConnection(connectionString);

        // Validar Login recuperando Rol y Estado [cite: 62, 67]
        public string ValidarLogin(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT rol, estado FROM Usuarios WHERE nombre_usuario = @u AND password = @p";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@p", password);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            if (r.GetInt32("estado") == 0) return "BANEADO"; // [cite: 80]
                            return r.GetString("rol"); // [cite: 58]
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error BBDD: " + ex.Message); }
                return null;
            }
        }

        // --- OPERACIONES CRUD (Punto 3) [cite: 68] ---

        public DataTable ObtenerUsuarios()
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT id, nombre_usuario, password, email, rol, estado FROM Usuarios";
                new MySqlDataAdapter(query, conn).Fill(dt);
            }
            return dt; // [cite: 76]
        }

        public bool AgregarUsuarioAdmin(string u, string p, string e, string r)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO Usuarios (nombre_usuario, password, email, rol, estado) VALUES (@u, @p, @e, @r, 1)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", u); cmd.Parameters.AddWithValue("@p", p);
                cmd.Parameters.AddWithValue("@e", e); cmd.Parameters.AddWithValue("@r", r);
                return cmd.ExecuteNonQuery() > 0; // [cite: 70]
            }
        }

        public bool EditarUsuario(int id, string p, string e, string r, int est)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "UPDATE Usuarios SET password=@p, email=@e, rol=@r, estado=@est WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@p", p); cmd.Parameters.AddWithValue("@e", e);
                cmd.Parameters.AddWithValue("@r", r); cmd.Parameters.AddWithValue("@est", est);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0; // [cite: 71, 79]
            }
        }

        public bool EliminarUsuario(int id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Usuarios WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0; // [cite: 74]
            }
        }

        // --- EXTENSIÓN: LOG DE ACTIVIDAD  ---
        public void RegistrarLog(string admin, string accion, string afectado)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO LogActividad (admin_usuario, accion, usuario_afectado) VALUES (@a, @ac, @af)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@a", admin);
                cmd.Parameters.AddWithValue("@ac", accion);
                cmd.Parameters.AddWithValue("@af", afectado);
                cmd.ExecuteNonQuery();
            }
        }
    }
}