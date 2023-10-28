using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Test.Manager;

namespace Test
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private SqlCommand cmd;
        private SqlConnection conn;
        public Login()
        {
            InitializeComponent();
            conn = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True");
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Btn_frogotPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ForgetPassword obj = new ForgetPassword();
            obj.ShowDialog();
        }

        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txt_userName.Text))
                    throw new Exception("Username cannot be blank.");

                if (string.IsNullOrEmpty(pwd_userPassword.Password))
                    throw new Exception("Password cannot be blank.");

                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM AdminLog WHERE adminUserID = @username AND adminPassword = @password", conn);
                cmd.Parameters.AddWithValue("@username", txt_userName.Text);
                cmd.Parameters.AddWithValue("@password", pwd_userPassword.Password);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read()) // Move to the first row
                    {
                        UserLog.userName = reader["adminUserID"].ToString();
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Hide(); // Hide the login window
                    }
                }
                else
                {
                    throw new Exception("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                conn.Close();
            }
        }

    }
}
    
