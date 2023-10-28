using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net;
using System.Net.Mail;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Windows.Markup;
using System.Security.Cryptography;

namespace Test.Manager
{
    /// <summary>
    /// Interaction logic for AddEmployee.xaml
    /// </summary>
    public partial class AddEmployee : UserControl
    {
        public AddEmployee()
        {
            InitializeComponent();
            dtp_dob.SelectedDateChanged += DtpDob_SelectedDateChanged;
            txt_age.IsReadOnly = true;
            GenerateAndDisplayCredentials();
        }
        DBConnection obj = new DBConnection();
        string psw;

        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gender = "";
                if (rd_female.IsChecked == true)
                    gender = rd_female.Content.ToString();
                else if (rd_male.IsChecked == true)
                    gender = rd_male.Content.ToString();
                else if (rd_other.IsChecked == true)
                    gender = rd_other.Content.ToString();

                string nicPattern = @"^[0-9]{9}[vVxX]$";
                DateTime maxDateOfBirth = DateTime.Today.AddYears(-60);
                if (string.IsNullOrEmpty(txt_first_name.Text) || txt_first_name.Text.Any(char.IsDigit))
                    throw new Exception("First Name cannot be blank OR cannot have numbers");

                else if (string.IsNullOrEmpty(txt_last_name.Text) || txt_last_name.Text.Any(char.IsDigit))
                    throw new Exception("Last Name cannot be blank OR cannot have numbers");

                else if (string.IsNullOrEmpty(txt_nic.Text) || !Regex.IsMatch(txt_nic.Text, nicPattern))
                    throw new Exception("Please Enter valid NIC Number AND cannot be blank");

                string designation = "";
                if (cmb_designation.SelectedIndex == 0)
                    designation = "Graphic Designer";
                else if (cmb_designation.SelectedIndex == 1)
                    designation = "Document Specialist(SD)";
                else if (cmb_designation.SelectedIndex == 2)
                    designation = "Senior Document Specialist(SDS)";
                else if (cmb_designation.SelectedIndex == 3)
                    designation = "Quality Control Specialist(QCS)";
                else
                    throw new Exception("Designation Cannot be Blank");

                string nic = txt_nic.Text;
                if (NicAlreadyExists(nic))
                {
                    throw new Exception("An employee with this NIC already exists.");
                }

                DateTime? selectedDate = dtp_dob.SelectedDate;
                if (selectedDate.HasValue)
                {
                    DateTime dob = selectedDate.Value;
                    int age = DateTime.Today.Year - dob.Year;

                    // Check if the birthday hasn't occurred this year
                    if (dob > DateTime.Today.AddYears(-age))
                        age--;

                    if (age < 18 || age >= 60)
                    {
                        throw new Exception("Age should be between 18 and 59.");
                    }

                    txt_age.Text = age.ToString();
                }
                else
                {
                    throw new Exception("Please select a valid date of birth.");
                }

                if (!rd_male.IsChecked.Value && !rd_female.IsChecked.Value && !rd_other.IsChecked.Value)
                    throw new Exception("Please select a gender");

                else if (string.IsNullOrEmpty(txt_address.Text))
                    throw new Exception("Address cannot be blank.");

                else if (string.IsNullOrEmpty(txt_tp.Text) || !IsSriLankanPhoneNumber(txt_tp.Text))
                {
                    if (txt_tp.Text.Any(char.IsLetter))
                        throw new Exception("Please enter a valid Sri Lankan phone number");
                }
                else if (string.IsNullOrEmpty(txt_email.Text))
                    throw new Exception("Email cannot be blank");

                else if (!IsValidEmail(txt_email.Text))
                    throw new Exception("Please enter a valid email address with the @gmail.com domain");

                else
                {
                    // Generate user ID and password
                    string userId = GenerateUserId();
                    string password = GeneratePassword();

                    // Encrypt the password before storing it in the database
                    string encryptedPassword = EncryptPassword(password);


                    // Display the generated credentials
                    txt_user_id.Text = userId;
                    pwd_password.Password = password;

                    string dob = dtp_dob.SelectedDate.Value.ToString("yyyy-MM-dd"); // Convert to "yyyy-MM-dd" format

                    string q = "Insert into Employee Values('" + userId + "','" + txt_first_name.Text + "','" + txt_last_name.Text + "','" + txt_nic.Text + "','" + dob + "','" + txt_age.Text + "','" + gender + "','" + txt_address.Text + "','" + designation + "','" + txt_email.Text + "','" + txt_tp.Text + "','" + DateTime.Now.Year + "')"; if (obj.insert_update_delete(q) == 1)
                    {
                        // Create PDF document
                        string filePath = GeneratePDF(userId, txt_first_name.Text, txt_last_name.Text, txt_nic.Text);

                        // Send registration email with PDF attachment
                        string emailSubject = "Registration Successful";
                        string emailBody = $"Registration successful." +
                            $"Dear Employee,\r\n\r\nWelcome to our organization! We are pleased to inform you that your registration has been successful." +
                            $"\r\n\r\nThe password is last four Characters of your registered ID number.\r\n\r\nExample: \r\n  If your ID No: 712233445V- " +
                            $"your password will be 445V\r\n\r\n  If your ID No: N2556159- your password will be 6159\r\n\r\n(Please note – The “V” or the " +
                            $"“X” in the NIC number must be typed in upper case)\r\n\r\n\r\nPlease keep these credentials secure and do not share them with " +
                            $"anyone.\r\n\r\nIf you have any questions or require further assistance, please feel free to contact our HR " +
                            $"department.\r\n\r\nThis is an automated email and does not require any authorized signature.\r\n\r\nThank " +
                            $"you for joining our team.\r\n\r\nBest regards,\r\n\r\nRR Donnelley Outsource Pvt Ltd,\r\nLevel 33, East " +
                            $"Tower, World Trade Center, Echelon Square,\r\nColombo, Sri Lanka,\r\n\r\n\r\nhttp://rrd.com";

                        string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your email address 
                        string senderPassword = "cvonpgclxinqibnu"; // Replace with your email password
                        string recipientEmail = txt_email.Text;

                        using (MailMessage mail = new MailMessage(senderEmail, recipientEmail, emailSubject, emailBody))
                        {
                            // Attach the PDF file
                            Attachment attachment = new Attachment(filePath);
                            mail.Attachments.Add(attachment);

                            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // Replace with your email provider's SMTP server and port number
                            smtp.EnableSsl = true;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                            smtp.Send(mail);
                        }
                        string qa = "Insert Into Employee_Login_Info values ('" + userId + "','" + encryptedPassword + "')";
                        obj.insert_update_delete(qa);

                        InsertOrUpdateAnnualLeaves(userId);

                        MessageBox.Show("Registration successful. An email has been sent with your login details.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        txt_first_name.Clear();
                        txt_last_name.Clear();
                        txt_user_id.Clear();
                        pwd_password.Clear();
                        txt_nic.Clear();
                        dtp_dob.SelectedDate = null;
                        txt_age.Clear();
                        txt_email.Clear();
                        txt_tp.Clear();
                        txt_address.Clear();
                        rd_female.IsChecked = false;
                        rd_male.IsChecked = false;
                        rd_other.IsChecked = false;
                        cmb_designation.SelectedIndex = -1;

                        // Regenerate User ID
                        txt_user_id.Text = GenerateUserId();
                        pwd_password.Password = GeneratePassword();
                    }
                    else
                        MessageBox.Show("Registration Unsuccessful", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    obj.bdClose();
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("Please Check the Format", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error Occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DtpDob_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = dtp_dob.SelectedDate;
            if (selectedDate.HasValue)
            {
                DateTime dob = selectedDate.Value;
                int age = DateTime.Today.Year - dob.Year;

                // Check if the birthday hasn't occurred this year
                if (dob > DateTime.Today.AddYears(-age))
                    age--;

                txt_age.Text = age.ToString();
            }
        }

        private void GenerateAndDisplayCredentials()
        {
            string userId = GenerateUserId();
            string password = GeneratePassword();

            txt_user_id.Text = userId;
            pwd_password.Password = password;
        }

        private string GenerateUserId()
        {
            // Logic to generate a unique user ID
            // Example: You can use a combination of initials, random numbers, or other relevant information

            // Get the last inserted user ID from the database
            string lastUserIdQuery = "SELECT TOP 1 Employee_Id FROM Employee ORDER BY Employee_Id DESC";
            string lastUserId = null;

            using (SqlConnection connection = new SqlConnection("Data Source = DILA99; Initial Catalog = RRD; Integrated Security = True"))
            {
                connection.Open();

                SqlCommand lastUserIdCommand = new SqlCommand(lastUserIdQuery, connection);
                SqlDataReader reader = lastUserIdCommand.ExecuteReader();

                if (reader.Read())
                {
                    lastUserId = reader["Employee_Id"].ToString();
                }

                reader.Close();
            }

            if (!string.IsNullOrEmpty(lastUserId))
            {
                // Extract the numeric portion of the user ID
                string numericPart = lastUserId.Split('_')[1].Substring(0, 3);
                int employeeNumber = int.Parse(numericPart) + 1; // Increment the employee number

                string initials = "CORRD";
                string identifier = "EMP";
                return $"{initials}_{employeeNumber:D3}{identifier}";
            }
            else
            {
                // If there are no existing user IDs, generate the first user ID
                string initials = "CORRD";
                int employeeNumber = 1;
                string identifier = "EMP";
                return $"{initials}_{employeeNumber:D3}{identifier}";
            }
        }

        private string GeneratePassword()
        {
            // Logic to generate a strong password
            // Example: You can use a combination of random characters, numbers, and symbols
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            Random random = new Random();
            StringBuilder passwordBuilder = new StringBuilder();

            for (int i = 0; i < 8; i++)
            {
                int index = random.Next(validChars.Length);
                passwordBuilder.Append(validChars[index]);
            }

            return passwordBuilder.ToString();
        }

        private bool ContainsSymbolsOrEmoji(string text)
        {
            // Use regular expression or other logic to check if the text contains symbols or emojis
            // Implement your own logic here
            // Example: You can use regex pattern to check for symbols or emojis
            const string pattern = @"[\p{S}\p{P}\p{C}]";
            return Regex.IsMatch(text, pattern);
        }

        private bool IsSriLankanPhoneNumber(string text)
        {
            // Use regular expression or other logic to validate Sri Lankan phone number
            // Implement your own logic here
            // Example: Validate if the phone number starts with "+94" and has 10 digits
            const string pattern = @"^(?:0|(\+?94)|0094)\s?7\d{8}$";
            return Regex.IsMatch(text, pattern);
        }

        private bool IsValidEmail(string text)
        {
            // Use regular expression or other logic to validate email address with @gmail.com domain
            // Implement your own logic here
            // Example: Validate if the email address follows the standard format and has the @gmail.com domain
            const string pattern = @"^[a-zA-Z0-9._%+-]+@gmail.com$";
            return Regex.IsMatch(text, pattern);
        }


        private string GeneratePDF(string userId, string firstName, string lastName, string nic)
        {
            string filePath = "employee_details.pdf";
            GeneratePassword();//Call generate password method

            // Encrypt the PDF with the last 4 characters of NIC as the password
            string password = nic.Substring(nic.Length - 4);

            using (var document = new Document())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                writer.SetEncryption(Encoding.ASCII.GetBytes(password), Encoding.ASCII.GetBytes(password), PdfWriter.AllowPrinting, PdfWriter.ENCRYPTION_AES_128);

                document.Open();

                // Add the watermark to each page
                PdfContentByte canvas;
                for (int pageNumber = 1; pageNumber <= writer.PageNumber; pageNumber++)
                {
                    canvas = writer.DirectContentUnder;
                    Rectangle pageSize = document.PageSize;

                    float x = pageSize.Width / 2;
                    float y = pageSize.Height / 2;

                    PdfGState gState = new PdfGState();
                    gState.FillOpacity = 0.5f; // Adjust the opacity as desired
                    canvas.SetGState(gState);

                    // Set the watermark text and properties
                    string watermarkText = "RR Donnelley";
                    iTextSharp.text.Font watermarkFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 85, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.LIGHT_GRAY);
                    Phrase watermark = new Phrase(watermarkText, watermarkFont);

                    ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, watermark, x, y, 45); // Rotate the watermark if desired
                }

                // Create a font for the title and content
                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 16, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
                iTextSharp.text.Font contentFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

                // Add the title
                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Your Employee Login Details", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 10;
                document.Add(title);

                // Add the content for page 1
                iTextSharp.text.Paragraph content = new iTextSharp.text.Paragraph();
                content.Alignment = iTextSharp.text.Element.ALIGN_JUSTIFIED;
                content.IndentationLeft = 20;
                content.IndentationRight = 20;

                content.Add($"Dear {firstName} {lastName},\n\n");
                content.Add("We are excited to welcome you as a new member of our team!\n\n");
                content.Add("Below are the details you'll need to access our system:\n\n");

                content.Add("Your Employee User ID and password to access our system are:\n\n");
                content.Add($"User ID: {userId}\n");
                content.Add($"Password: {pwd_password.Password}\n\n");

                content.Add("Please keep this information confidential and refrain from sharing it with anyone else. These credentials are essential for logging in to our systems securely. " +
                    "" + DateTime.Now.ToString("MMMM dd, yyyy") + ". If you have any technical difficulties or questions about " +
                    "the login process, feel free to reach out to our IT support team at [IT Support Contact] for assistance.\n\n");

                content.Add("Best regards,\n\n");
                content.Add("RR Donnelley\n");
                document.Add(content);

                // Add more pages if needed
                for (int i = 2; i <= 3; i++)
                {
                    document.NewPage();
                    // Add content for the additional pages
                    // ...
                }

                document.Close();
            }

            return filePath;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_first_name.Clear();
            txt_last_name.Clear();
            txt_user_id.Clear();
            pwd_password.Clear();
            txt_nic.Clear();
            dtp_dob.SelectedDate = null;
            txt_age.Clear();
            txt_email.Clear();
            txt_tp.Clear();
            txt_address.Clear();
            rd_female.IsChecked = false;
            rd_male.IsChecked = false;
            rd_other.IsChecked = false;
            cmb_designation.SelectedIndex = -1;

            // Regenerate User ID
            txt_user_id.Text = GenerateUserId();
            pwd_password.Password = GeneratePassword();
        }

        private void InsertOrUpdateAnnualLeaves(string employeeId)
        {
            try
            {
                // Check if the employee already exists in the AnnualLeave table
                string checkQuery = "SELECT COUNT(*) FROM AnnualLeave WHERE Employee_Id = @EmployeeId";

                using (SqlConnection connection = new SqlConnection("Data Source = DILA99; Initial Catalog = RRD; Integrated Security = True"))
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
                        int existingEmployeeCount = (int)checkCommand.ExecuteScalar();

                        // If the employee doesn't exist in the AnnualLeave table, insert the annual leave data
                        if (existingEmployeeCount == 0)
                        {
                            string insertAnnualLeaveQuery = @"
                        INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
                        VALUES (@EmployeeId, 'Annual', 20, GETDATE()),
                               (@EmployeeId, 'Sick', 14, GETDATE()),
                               (@EmployeeId, 'Personal', 5, GETDATE()),
                               (@EmployeeId, 'Special', 2, GETDATE())";

                            using (SqlCommand insertCommand = new SqlCommand(insertAnnualLeaveQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                        else // Employee already exists in AnnualLeave table, update the existing annual leave data
                        {
                            string updateAnnualLeaveQuery = @"
                        UPDATE AnnualLeave
                        SET LeaveBalance = 
                            CASE 
                                WHEN LeaveType = 'Annual' THEN 20
                                WHEN LeaveType = 'Sick' THEN 14
                                WHEN LeaveType = 'Personal' THEN 5
                                WHEN LeaveType = 'Special' THEN 2
                                ELSE LeaveBalance -- To handle other leave types, if any
                            END,
                        LastUpdated = GETDATE()
                        WHERE Employee_Id = @EmployeeId";

                            using (SqlCommand updateCommand = new SqlCommand(updateAnnualLeaveQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error Occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool NicAlreadyExists(string nic)
        {
            string query = "SELECT COUNT(*) FROM Employee WHERE NIC_Number = @NIC";

            using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@NIC", nic);

                int count = (int)command.ExecuteScalar();

                return count > 0;
            }
        }

        private string EncryptPassword(string password)
        {
            // Create an instance of SHA256 for hashing
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the password string to byte array
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);

                // Compute the hash
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert the hash byte array to a hexadecimal string
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }

    }
}