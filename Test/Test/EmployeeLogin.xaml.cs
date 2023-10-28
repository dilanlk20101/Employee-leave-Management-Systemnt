using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Test
{
    public partial class EmployeeLogin : Window
    {
        private SqlConnection conn;
        public EmployeeLogin()
        {
            InitializeComponent();
            conn = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True");
        }

        private void Btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_forgotPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ForgetPassword obj = new ForgetPassword();
            obj.ShowDialog();
            this.Show();
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM Employee_Login_Info WHERE Employee_LoginId = @username AND User_Password = @password", conn);
                cmd.Parameters.AddWithValue("@username", txt_userName.Text);
                cmd.Parameters.AddWithValue("@password", EncryptPassword(pwd_userPassword.Password));

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read()) // Move to the first row
                    {
                        UserLog.userName = reader["Employee_LoginId"].ToString();

                        EmployeeHome Eh = new EmployeeHome();
                        Eh.Show();
                        this.Close();
                    }
                }
                else
                {
                    throw new Exception("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private string EncryptPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void Btn_frogotPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ForgetPassword obj = new ForgetPassword();
            obj.ShowDialog();
            this.Show();
        }
    }
}
