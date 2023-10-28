using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace Test.Manager
{
    /// <summary>
    /// Interaction logic for UpdateEmployeexaml.xaml
    /// </summary>
    public partial class UpdateEmployeexaml : UserControl
    {
        private ObservableCollection<EmployeeUpdate> employeeUpdates;

        public UpdateEmployeexaml()
        {
            InitializeComponent();
            employeeUpdates = new ObservableCollection<EmployeeUpdate>();
            dg_emp.ItemsSource = employeeUpdates;

            // Load the employee updates initially
            LoadEmployeeUpdates();
        }

        private void LoadEmployeeUpdates()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string query = "SELECT * FROM EmployeeUpdate";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string employeeId = reader["EmployeeId"].ToString();
                                string updateDetails = reader["UpdateDetails"].ToString();
                                DateTime updateDateTime = Convert.ToDateTime(reader["UpdateDateTime"]);

                                EmployeeUpdate employeeUpdate = new EmployeeUpdate(employeeId, updateDetails, updateDateTime);
                                employeeUpdates.Add(employeeUpdate);
                            }
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

        private void Btn_empSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string empId = txt_emp_id.Text;

                if (string.IsNullOrEmpty(empId))
                {
                    MessageBox.Show("Employee ID cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string query = "SELECT * FROM Employee WHERE Employee_Id = @EmpId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmpId", empId);
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            txt_first_name.Text = reader["First_Name"].ToString();
                            txt_last_name.Text = reader["Last_Name"].ToString();
                            cmb_designation.SelectedItem = reader["Designation"].ToString();
                            txt_nic.Text = reader["NIC_Number"].ToString();
                            txt_email.Text = reader["Email"].ToString();
                            txt_address.Text = reader["Employee_Address"].ToString();
                            dtp_dob.SelectedDate = Convert.ToDateTime(reader["DOB"]);
                            txt_age.Text = reader["Age"].ToString();
                            string gender = reader["Gender"].ToString();
                            if (gender == "Male")
                                rd_male.IsChecked = true;
                            else if (gender == "Female")
                                rd_female.IsChecked = true;
                            else if (gender == "Other")
                                rd_other.IsChecked = true;
                            txt_tp.Text = reader["TP"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            ClearFormFields();
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

        private void Btn_empUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string empId = txt_emp_id.Text;

                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string retrieveQuery = "SELECT * FROM Employee WHERE Employee_Id = @EmpId";
                    using (SqlCommand retrieveCommand = new SqlCommand(retrieveQuery, connection))
                    {
                        retrieveCommand.Parameters.AddWithValue("@EmpId", empId);
                        SqlDataReader reader = retrieveCommand.ExecuteReader();

                        MessageBoxResult confirmResult = MessageBox.Show("Are you sure you want to update this employee's details?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (reader.Read())
                        {
                            string firstName = txt_first_name.Text;
                            if (string.IsNullOrEmpty(firstName))
                                firstName = reader["First_Name"].ToString();

                            string lastName = txt_last_name.Text;
                            if (string.IsNullOrEmpty(lastName))
                                lastName = reader["Last_Name"].ToString();

                            string designation = cmb_designation.SelectedItem?.ToString();
                            if (string.IsNullOrEmpty(designation))
                                designation = reader["Designation"].ToString();

                            string nic = txt_nic.Text;
                            if (string.IsNullOrEmpty(nic))
                                nic = reader["NIC_Number"].ToString();

                            string email = txt_email.Text;
                            if (string.IsNullOrEmpty(email))
                                email = reader["Email"].ToString();

                            string address = txt_address.Text;
                            if (string.IsNullOrEmpty(address))
                                address = reader["Employee_Address"].ToString();

                            DateTime dob = dtp_dob.SelectedDate.HasValue ? dtp_dob.SelectedDate.Value : Convert.ToDateTime(reader["DOB"]);

                            int age;
                            if (!int.TryParse(txt_age.Text, out age))
                                age = Convert.ToInt32(reader["Age"]);

                            string gender = GetSelectedGender();
                            if (string.IsNullOrEmpty(gender))
                                gender = reader["Gender"].ToString();

                            string tp = txt_tp.Text;
                            if (string.IsNullOrEmpty(tp))
                                tp = reader["TP"].ToString();

                            reader.Close();

                            string updateQuery = "UPDATE Employee SET First_Name = @FirstName, Last_Name = @LastName, Designation = @Designation, NIC_Number = @NIC, Email = @Email, Employee_Address = @Address, DOB = @DOB, Age = @Age, Gender = @Gender, TP = @TP WHERE Employee_Id = @EmpId";
                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@FirstName", firstName);
                                updateCommand.Parameters.AddWithValue("@LastName", lastName);
                                updateCommand.Parameters.AddWithValue("@Designation", designation);
                                updateCommand.Parameters.AddWithValue("@NIC", nic);
                                updateCommand.Parameters.AddWithValue("@Email", email);
                                updateCommand.Parameters.AddWithValue("@Address", address);
                                updateCommand.Parameters.AddWithValue("@DOB", dob);
                                updateCommand.Parameters.AddWithValue("@Age", age);
                                updateCommand.Parameters.AddWithValue("@Gender", gender);
                                updateCommand.Parameters.AddWithValue("@TP", tp);
                                updateCommand.Parameters.AddWithValue("@EmpId", empId);

                                if (updateCommand.ExecuteNonQuery() > 0)
                                {
                                    string updateDetails = "Employee details updated.";
                                    DateTime updateDateTime = DateTime.Now;

                                    string insertUpdateQuery = "INSERT INTO EmployeeUpdate (EmployeeId, UpdateDetails, UpdateDateTime) VALUES (@EmpId, @UpdateDetails, @UpdateDateTime)";
                                    using (SqlCommand insertUpdateCommand = new SqlCommand(insertUpdateQuery, connection))
                                    {
                                        insertUpdateCommand.Parameters.AddWithValue("@EmpId", empId);
                                        insertUpdateCommand.Parameters.AddWithValue("@UpdateDetails", updateDetails);
                                        insertUpdateCommand.Parameters.AddWithValue("@UpdateDateTime", updateDateTime);

                                        insertUpdateCommand.ExecuteNonQuery();
                                    }

                                    string emailSubject = "Employee Details Updated";
                                    string emailBody = $"Dear Employee,\n\nYour employee details have been updated. Please review the updated information to ensure its accuracy.\n\nUpdated fields: {updateDetails}\n\nThank you.";

                                    string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your email address
                                    string senderPassword = "cvonpgclxinqibnu"; // Replace with your email password
                                    string recipientEmail = txt_email.Text;

                                    using (MailMessage mail = new MailMessage(senderEmail, recipientEmail, emailSubject, emailBody))
                                    {
                                        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // Replace with your email provider's SMTP server and port number
                                        smtp.EnableSsl = true;
                                        smtp.UseDefaultCredentials = false;
                                        smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                                        smtp.Send(mail);
                                    }

                                    MessageBox.Show("Employee details updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearFormFields();
                                    LoadEmployeeUpdates();
                                }
                                else
                                {
                                    MessageBox.Show("Error occurred while updating employee details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            reader.Close();
                            MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private string GetSelectedGender()
        {
            if (rd_male.IsChecked == true)
                return "Male";
            else if (rd_female.IsChecked == true)
                return "Female";
            else if (rd_other.IsChecked == true)
                return "Other";
            else
                return string.Empty;
        }

        private void ClearFormFields()
        {
            txt_emp_id.Clear();
            txt_first_name.Clear();
            txt_last_name.Clear();
            cmb_designation.SelectedIndex = -1;
            txt_nic.Clear();
            txt_email.Clear();
            txt_address.Clear();
            dtp_dob.SelectedDate = null;
            txt_age.Clear();
            rd_male.IsChecked = false;
            rd_female.IsChecked = false;
            rd_other.IsChecked = false;
            txt_tp.Clear();
        }

        // Class representing an employee update
        public class EmployeeUpdate
        {
            public string EmployeeId { get; set; }
            public string UpdateDetails { get; set; }
            public DateTime UpdateDateTime { get; set; }

            public EmployeeUpdate(string employeeId, string updateDetails, DateTime updateDateTime)
            {
                EmployeeId = employeeId;
                UpdateDetails = updateDetails;
                UpdateDateTime = updateDateTime;
            }
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_first_name.Clear();
            txt_last_name.Clear();
            txt_emp_id.Clear();
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
        }
    }
}