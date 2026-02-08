using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace LoginPractica
{
    public class DatabaseHelper
    {
        private string connectionString = "Server=localhost;Database=PracticaLoginDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection() => new MySqlConnection(connectionString);

        // --- LOGIN MODIFICADO PARA DETECTAR ERROR DE CONTRASEÑA ---
        public string ValidarLogin(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // 1. Buscamos al usuario por su nombre (SIN comprobar la contraseña aún)
                    string query = "SELECT password, rol, estado FROM Usuarios WHERE nombre_usuario = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read()) // El usuario EXISTE
                        {
                            string dbPass = r.GetString("password");
                            int estado = r.GetInt32("estado");

                            // 2. Comprobamos la contraseña nosotros
                            if (dbPass == password)
                            {
                                if (estado == 0) return "BANEADO";
                                return r.GetString("rol"); // Login Correcto
                            }
                            else
                            {
                                return "BAD_PASS"; // Usuario bien, contraseña mal
                            }
                        }
                        else
                        {
                            return "NO_USER"; // Usuario no encontrado
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error BBDD: " + ex.Message); }
                return null;
            }
        }

        // --- RESTO DE MÉTODOS (SIN CAMBIOS) ---

        public DataTable ObtenerUsuarios()
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                new MySqlDataAdapter("SELECT * FROM Usuarios", conn).Fill(dt);
            }
            return dt;
        }

        public bool GuardarUsuario(int? id, string u, string p, string e, string r, int est)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = id == null
                    ? "INSERT INTO Usuarios (nombre_usuario, password, email, rol, estado) VALUES (@u, @p, @e, @r, @est)"
                    : "UPDATE Usuarios SET nombre_usuario=@u, password=@p, email=@e, rol=@r, estado=@est WHERE id=@id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                if (id != null) cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@u", u); cmd.Parameters.AddWithValue("@p", p);
                cmd.Parameters.AddWithValue("@e", e); cmd.Parameters.AddWithValue("@r", r);
                cmd.Parameters.AddWithValue("@est", est);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool AgregarUsuarioAdmin(string u, string p, string e, string r) => GuardarUsuario(null, u, p, e, r, 1);
        public bool EditarUsuario(int id, string p, string e, string r, int est) => GuardarUsuario(id, null, p, e, r, est);

        public bool EliminarUsuario(int id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                return new MySqlCommand($"DELETE FROM Usuarios WHERE id={id}", conn).ExecuteNonQuery() > 0;
            }
        }

        public void RegistrarLog(string admin, string accion, string afectado)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string q = "INSERT INTO LogActividad (admin_usuario, accion, usuario_afectado) VALUES (@a, @ac, @af)";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@a", admin); cmd.Parameters.AddWithValue("@ac", accion);
                cmd.Parameters.AddWithValue("@af", afectado);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Juego> ObtenerJuegos()
        {
            List<Juego> lista = new List<Juego>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (MySqlDataReader r = new MySqlCommand("SELECT * FROM Juegos", conn).ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new Juego
                            {
                                Id = r.GetInt32("id"),
                                Titulo = r.GetString("titulo"),
                                Descripcion = r.IsDBNull(r.GetOrdinal("descripcion")) ? "" : r.GetString("descripcion"),
                                Precio = r.GetDecimal("precio"),
                                ImagenUrl = r.GetString("imagen_url"),
                                Categoria = r.GetString("categoria")
                            });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        public List<int> ObtenerIdsMisJuegos(string nombreUsuario)
        {
            List<int> ids = new List<int>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id_juego FROM Compras WHERE id_usuario = (SELECT id FROM Usuarios WHERE nombre_usuario = @u)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", nombreUsuario);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read()) ids.Add(r.GetInt32("id_juego"));
                    }
                }
                catch { }
            }
            return ids;
        }

        public bool ConfirmarCompraCarrito(string nombreUsuario, List<Juego> carrito)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmdUser = new MySqlCommand("SELECT id FROM Usuarios WHERE nombre_usuario = @u", conn, transaction);
                    cmdUser.Parameters.AddWithValue("@u", nombreUsuario);
                    int idUser = Convert.ToInt32(cmdUser.ExecuteScalar());

                    foreach (var juego in carrito)
                    {
                        string q = "INSERT INTO Compras (id_usuario, id_juego, importe) VALUES (@uid, @jid, @imp)";
                        MySqlCommand cmd = new MySqlCommand(q, conn, transaction);
                        cmd.Parameters.AddWithValue("@uid", idUser);
                        cmd.Parameters.AddWithValue("@jid", juego.Id);
                        cmd.Parameters.AddWithValue("@imp", juego.Precio);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return true;
                }
                catch { transaction.Rollback(); return false; }
            }
        }

        public bool AgregarJuego(string t, string d, decimal p, string c, string i)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string q = "INSERT INTO Juegos (titulo, descripcion, precio, categoria, imagen_url) VALUES (@t, @d, @p, @c, @i)";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@t", t); cmd.Parameters.AddWithValue("@d", d);
                    cmd.Parameters.AddWithValue("@p", p); cmd.Parameters.AddWithValue("@c", c);
                    cmd.Parameters.AddWithValue("@i", i);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool EditarJuego(int id, string t, string d, decimal p, string c, string i)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string q = "UPDATE Juegos SET titulo=@t, descripcion=@d, precio=@p, categoria=@c, imagen_url=@i WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@t", t); cmd.Parameters.AddWithValue("@d", d);
                    cmd.Parameters.AddWithValue("@p", p); cmd.Parameters.AddWithValue("@c", c);
                    cmd.Parameters.AddWithValue("@i", i);
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool EliminarJuego(int id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                new MySqlCommand($"DELETE FROM Compras WHERE id_juego={id}", conn).ExecuteNonQuery();
                return new MySqlCommand($"DELETE FROM Juegos WHERE id={id}", conn).ExecuteNonQuery() > 0;
            }
        }
    }
}