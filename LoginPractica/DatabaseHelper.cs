using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;

namespace LoginPractica
{
    public class DatabaseHelper
    {
        // CAMBIA "Pwd=;" por tu contraseña de MySQL si tienes una (ej: Pwd=root;)
        private string connectionString = "Server=localhost;Database=PracticaLoginDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // VALIDAR LOGIN (SELECT)
        public bool ValidarLogin(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @user AND password = @pass";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", usuario);
                    cmd.Parameters.AddWithValue("@pass", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de conexión BBDD: " + ex.Message);
                    return false;
                }
            }
        }

        // REGISTRAR USUARIO (INSERT)
        public bool RegistrarUsuario(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // 1. Verificar si ya existe
                    string checkQuery = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @user";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@user", usuario);
                    int exist = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exist > 0)
                    {
                        MessageBox.Show("El nombre de usuario ya existe. Prueba otro.");
                        return false;
                    }

                    // 2. Insertar nuevo
                    string query = "INSERT INTO Usuarios (nombre_usuario, password) VALUES (@user, @pass)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", usuario);
                    cmd.Parameters.AddWithValue("@pass", password);

                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al registrar: " + ex.Message);
                    return false;
                }
            }
        }
    }
}