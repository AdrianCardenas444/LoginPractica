using System;
using System.Windows;
using System.Windows.Controls;

namespace LoginPractica
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper db = new DatabaseHelper();

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- INICIO DE SESIÓN ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Validamos
            string resultado = db.ValidarLogin(txtUser.Text, txtPass.Password);

            if (resultado == "BANEADO")
            {
                lblError.Text = "ACCESO DENEGADO: Tu cuenta está baneada.";
            }
            else if (resultado == "BAD_PASS")
            {
                // EL MENSAJE QUE PEDISTE
                lblError.Text = "La contraseña introducida no es correcta.";
                txtPass.Clear();
                txtPass.Focus();
            }
            else if (resultado == "NO_USER")
            {
                lblError.Text = "El nombre de usuario no existe.";
                txtUser.Focus();
            }
            else if (resultado != null)
            {
                // Login Correcto
                new HomeWindow(txtUser.Text, resultado).Show();
                this.Close();
            }
            else
            {
                lblError.Text = "Error de conexión con la base de datos.";
            }
        }

        // --- CREAR CUENTA (RECUPERADO) ---
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Password))
            {
                lblError.Text = "Para crear cuenta necesitas Usuario y Contraseña.";
                return;
            }

            // Intentamos guardar un usuario nuevo (Rol 'user' por defecto)
            // Usamos el método GuardarUsuario que ya tienes en DatabaseHelper
            // GuardarUsuario(id, user, pass, email, rol, estado)
            bool exito = db.GuardarUsuario(null, txtUser.Text, txtPass.Password, txtEmail.Text, "user", 1);

            if (exito)
            {
                MessageBox.Show("¡Cuenta creada con éxito! Ahora puedes iniciar sesión.");
                lblError.Text = "";
            }
            else
            {
                lblError.Text = "Error: El nombre de usuario ya existe.";
            }
        }

        // --- SALIR (RECUPERADO) ---
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}