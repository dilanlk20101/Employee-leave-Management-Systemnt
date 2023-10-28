using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace Test
{
    public partial class ForgetPassword : Window
    {
        private string employeeEmail;
        private string employeeMobileNumber;
        private string otp;
        private bool isOTPVerified = false;

        public ForgetPassword()
        {
            InitializeComponent();
            canvas_changePassword.Visibility = Visibility.Collapsed;
        }

        private void Btn_reqOTP_Click(object sender, RoutedEventArgs e)
        {
            string username = txt_userName.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter a username.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the username exists in the Employee table
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string selectQuery = "SELECT Email, TP FROM Employee WHERE Employee_Id = @Username";
                    using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employeeEmail = reader["Email"].ToString();
                                employeeMobileNumber = reader["TP"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Generate OTP
            otp = GenerateOTP();

            // Send OTP to email and mobile number
            SendOTPToEmail(otp);
            SendOTPToMobileNumber(otp);

            isOTPVerified = false;

            // Show success message when OTP is sent successfully
            MessageBox.Show("OTP sent successfully to your email.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Btn_verify_Click(object sender, RoutedEventArgs e)
        {
            string enteredOTP = txt_otp.Text;

            if (string.IsNullOrEmpty(txt_userName.Text))
            {
                MessageBox.Show("Please enter a User Name and request an OTP first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (enteredOTP == otp)
            {
                MessageBox.Show("OTP verification successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txt_otp.Clear();
                pwd_password.Clear();
                pwd_changePwd.Clear();
                isOTPVerified = true;
            }
            else
            {
                MessageBox.Show("Invalid OTP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                isOTPVerified = false;
            }

            // Show change password UI only if OTP is verified
            if (isOTPVerified)
            {
                canvas_changePassword.Visibility = Visibility.Visible;
                // Show verification UI
                canvas_forget.Visibility = Visibility.Hidden;
            }
        }

        private void btn_resetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!isOTPVerified)
            {
                MessageBox.Show("Please enter the correct OTP first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newPassword = pwd_changePwd.Password;

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter a new password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsPasswordValid(newPassword))
            {
                MessageBox.Show("Password should have at least 8 characters with minimum one number, one capital letter, and one symbol.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string confirmPassword = pwd_password.Password;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string username = txt_userName.Text;

            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string updateQuery = "UPDATE Employee_Login_Info SET User_Password = @NewPassword WHERE Employee_LoginId = @Username";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@NewPassword", EncryptPassword(newPassword));
                        updateCommand.Parameters.AddWithValue("@Username", username);
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Password reset successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            txt_userName.Clear();
                            txt_otp.Clear();
                            pwd_password.Clear();
                            pwd_changePwd.Clear();
                            SelectLogin sl = new SelectLogin();
                            sl.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Error occurred while resetting the password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            int otpValue = random.Next(100000, 999999);
            return otpValue.ToString();
        }

        private void SendOTPToEmail(string otp)
        {
            string emailSubject = "OTP for Password Reset";
            string emailBody = $"Your OTP for password reset is: {otp}";

            string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your email address
            string senderPassword = "cvonpgclxinqibnu"; // Replace with your email password

            try
            {
                using (MailMessage mail = new MailMessage(senderEmail, employeeEmail, emailSubject, emailBody))
                {
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // Replace with your email provider's SMTP server and port number
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while sending OTP: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendOTPToMobileNumber(string otp)
        {
            // Use your preferred method to send OTP to the mobile number
        }

        private string EncryptPassword(string password)
        {
            // Use your preferred encryption algorithm to encrypt the password
            // Example: SHA256 encryption
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

        private bool IsPasswordValid(string password)
        {
            // Password should have at least 8 characters with minimum one number, one capital letter, and one symbol
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d\s:]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Login login = new Login();
            login.Show();
        }

        private void btn_Chgback_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Login login = new Login();
            login.Show();
        }
    }
}
