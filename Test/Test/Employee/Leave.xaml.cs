using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Test.Employee
{
    public partial class Leave : UserControl
    {
        private DBConnection obj = new DBConnection();
        private ObservableCollection<AnnualLeave> annualLeaves = new ObservableCollection<AnnualLeave>();


        public Leave()
        {
            InitializeComponent();
            txt_empId.IsReadOnly = true;
            Loaded += Leave_Load;
            LoadLeaveBalance();
        }

        private void Leave_Load(object sender, RoutedEventArgs e)
        {
            txt_empId.Text = UserLog.userName.ToString();
            cmb_leaveType.Items.Add("Annual Leave");
            cmb_leaveType.Items.Add("Sick Leave");
            cmb_leaveType.Items.Add("Personal Leave");
            cmb_leaveType.Items.Add("Special Leave");
            datePicker_from.SelectedDateChanged += DatePicker_SelectedDateChanged;
            datePicker_to.SelectedDateChanged += DatePicker_SelectedDateChanged;
            CalculateLeaveDays();
            LoadLeaveBalance();
        }

        private void cmb_leaveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateLeaveDays();
        }

        private void datePicker_from_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateLeaveDays();
        }

        private void datePicker_to_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateLeaveDays();
        }

        private void CalculateLeaveDays()
        {
            if (cmb_leaveType.SelectedItem == null || datePicker_from.SelectedDate == null || datePicker_to.SelectedDate == null)
                return;

            string leaveType = cmb_leaveType.SelectedItem.ToString();
            DateTime fromDate = datePicker_from.SelectedDate.Value.Date;
            DateTime toDate = datePicker_to.SelectedDate.Value.Date;

            if (fromDate <= toDate)
            {
                // Calculate the number of leave days (including both start and end dates)
                int leaveDays = (int)(toDate - fromDate).TotalDays + 1;
                txt_noOfDays.Text = leaveDays.ToString();
            }
            else
            {
                MessageBox.Show("Leave start date cannot be after the leave end date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_apply_Click(object sender, RoutedEventArgs e)
        {
            try

                //< ComboBoxItem Content = "Annual Leave" />
                //< ComboBoxItem Content = "Sick Leave" />
                //< ComboBoxItem Content = "Personal Leave" />
                //< ComboBoxItem Content = "Special Leave" />
            {

                if (datePicker_from.SelectedDate == null)
                    throw new Exception("Please select date From.");

                if (datePicker_from.SelectedDate == null)
                    throw new Exception("Please select date To.");

                DateTime fromDate = datePicker_from.SelectedDate.Value;
                DateTime toDate = datePicker_to.SelectedDate.Value;

                if (fromDate < DateTime.Now.Date)
                    throw new Exception("From date must be today or later.");

                if (toDate <= fromDate)
                    throw new Exception("To date must be after the From date.");


                if(string.IsNullOrEmpty(txt_noOfDays.Text))
                    throw new Exception("Number of days canNot be Blank");

                int leaveDayss = (int)(toDate - fromDate).TotalDays + 1;

                // Check if the calculated leave days match the value inserted in txt_noOfDays
                if (leaveDayss != int.Parse(txt_noOfDays.Text))
                {
                    throw new Exception("The number of leave days does not match the selected duration. Please check the selected dates.");
                }

                if (!int.TryParse(txt_noOfDays.Text, out int leaveDays) || leaveDays <= 0)
                    throw new Exception("Invalid number of leave days. Please enter a valid positive integer.");

                else if (null == txt_noOfDays.Text) 
                {
                    throw new Exception("Number os days canNot be null");
                }

                if (string.IsNullOrEmpty(new TextRange(richTxt_Reson.Document.ContentStart, richTxt_Reson.Document.ContentEnd).Text) && cmb_leaveType.SelectedItem.ToString() == "Special Leave")
                    throw new Exception("Please provide a reason for Special Leave.");

                string dpTO = datePicker_to.SelectedDate.Value.ToString("yyyy-MM-dd"); // Convert to "yyyy-MM-dd" format
                string dpFrom = datePicker_from.SelectedDate.Value.ToString("yyyy-MM-dd"); // Convert to "yyyy-MM-dd" format

                string leaveId = GenerateLeaveId();
                string employeeId = txt_empId.Text;
                string leaveType = cmb_leaveType.SelectedItem.ToString(); // Get the selected leave type as a string
                string reason = new TextRange(richTxt_Reson.Document.ContentStart, richTxt_Reson.Document.ContentEnd).Text;
                string status = "Pending"; // Set the initial status as "Pending"
                string managerComment = ""; // Leave request hasn't been reviewed by the manager yet

                // Insert leave request details into the LeaveRequests table
                string query = "INSERT INTO LeaveRequests (Id, EmployeeId, LeaveType, FromDate, ToDate, Reason, Status, ManagerComment) " +
                               "VALUES (@LeaveId, @EmployeeId, @LeaveType, @FromDate, @ToDate, @Reason, @Status, @ManagerComment)";

                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@LeaveId", leaveId);
                    command.Parameters.AddWithValue("@EmployeeId", employeeId);
                    command.Parameters.AddWithValue("@LeaveType", leaveType);
                    command.Parameters.AddWithValue("@FromDate", dpFrom);
                    command.Parameters.AddWithValue("@ToDate", dpTO);
                    command.Parameters.AddWithValue("@Reason", reason);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@ManagerComment", managerComment);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Leave request submitted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Send email notification to the employee
                        SendLeaveRequestEmail(employeeId, leaveId, dpFrom, dpTO, leaveType);

                        // Clear form fields after successful submission
                        ClearFormFields();
                    }
                    else
                    {
                        MessageBox.Show("Leave request submission failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input. Please check the entered values.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error occurred while accessing the database: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendLeaveRequestEmail(string employeeId, string leaveId, string fromDate, string toDate, string leaveType)
        {
            string emailSubject = "Leave Request Submitted";
            string emailBody = $"Dear Employee,\r\n\r\nYour leave request with the following details has been submitted successfully:\r\n" +
                               $"Leave ID: {leaveId}\r\nEmployee ID: {employeeId}\r\nLeave Type: {leaveType}\r\n" +
                               $"From: {fromDate}\r\nTo: {toDate}\r\n\r\n" +
                               $"Please wait for the approval of your leave request.\r\n\r\n" +
                               $"Thank you.\r\n\r\nBest Regards,\r\nYour Company Name";

            // string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your email address
            // string senderPassword = "cvonpgclxinqibnu"; // Replace with your email password

            string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your company's email address
            string senderPassword = "cvonpgclxinqibnu"; // Replace with your company's email password

            // Fetch the recipient email from the database based on the employeeId
            string recipientEmail = GetEmployeeEmail(employeeId);
            if (string.IsNullOrEmpty(recipientEmail))
            {
                MessageBox.Show("Recipient email not found for the employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (MailMessage mail = new MailMessage(senderEmail, recipientEmail, emailSubject, emailBody))
            {
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587); // Replace with your email provider's SMTP server and port number
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtp.Send(mail);
            }
        }

        private string GetEmployeeEmail(string employeeId)
        {
            string recipientEmail = string.Empty;
            string query = $"SELECT Email FROM Employee WHERE Employee_Id = '{employeeId}'";

            using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    recipientEmail = reader["Email"].ToString();
                }

                reader.Close();
            }

            return recipientEmail;
        }

        private void ClearFormFields()
        {
            cmb_leaveType.SelectedIndex = -1;
            datePicker_from.SelectedDate = null;
            datePicker_to.SelectedDate = null;
            txt_noOfDays.Clear();
            richTxt_Reson.Document.Blocks.Clear();
        }

        private string GenerateLeaveId()
        {
            try
            {
                string lastLeaveIdQuery = "SELECT TOP 1 Id FROM LeaveRequests ORDER BY Id DESC";
                string lastLeaveId = null;

                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    SqlCommand lastLeaveIdCommand = new SqlCommand(lastLeaveIdQuery, connection);
                    SqlDataReader reader = lastLeaveIdCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        lastLeaveId = reader["Id"].ToString();
                    }

                    reader.Close();
                }

                // Generate the new leave ID based on the last leave ID
                int leaveNumber = 1;
                if (!string.IsNullOrEmpty(lastLeaveId))
                {
                    int lastNumber = int.Parse(lastLeaveId.Substring(3, 3));
                    leaveNumber = lastNumber + 1; // Increment the leave number
                }

                string newLeaveId = $"RRD{leaveNumber:D3}_eLeave";
                return newLeaveId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while generating Leave ID: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void LoadLeaveBalance()
        {
            try
            {
                string employeeId = txt_empId.Text;
                string query = $"SELECT LeaveType, LeaveBalance, LastUpdated FROM AnnualLeave WHERE Employee_Id = '{employeeId}'";

                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    annualLeaves.Clear(); // Clear the collection before loading new data

                    while (reader.Read())
                    {
                        string leaveType = reader["LeaveType"].ToString();
                        int leaveBalance = Convert.ToInt32(reader["LeaveBalance"]);
                        DateTime lastUpdated = Convert.ToDateTime(reader["LastUpdated"]);

                        AnnualLeave annualLeave = new AnnualLeave(leaveType, leaveBalance, lastUpdated);
                        annualLeaves.Add(annualLeave); // Add the object to the ObservableCollection
                    }

                    reader.Close();
                }

                // Bind the ObservableCollection to the DataGrid
                dg_leaveBalance.ItemsSource = annualLeaves;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error occurred while accessing the database: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class AnnualLeave
        {
            public string LeaveType { get; set; }
            public int LeaveBalance { get; set; }
            public DateTime LastUpdated { get; set; }

            public AnnualLeave(string leaveType, int leaveBalance, DateTime lastUpdated)
            {
                LeaveType = leaveType;
                LeaveBalance = leaveBalance;
                LastUpdated = lastUpdated;
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateLeaveDays();
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_empId.Clear();
            txt_noOfDays.Clear();
            cmb_leaveType.SelectedIndex = -1;
            datePicker_from.SelectedDate = null;
            datePicker_to.SelectedDate = null;
            richTxt_Reson.Document.Blocks.Clear();
        }
    }
}