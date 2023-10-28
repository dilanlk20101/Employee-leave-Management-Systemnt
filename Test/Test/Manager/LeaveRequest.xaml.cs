using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Test.Manager
{
    public partial class LeaveRequest : UserControl
    {
        private string managerId; // Assuming you have a way to get the manager's ID
        private ObservableCollection<LeaveRequestData> leaveRequests;

        public LeaveRequest()
        {
            InitializeComponent();
            Loaded += LeaveRequest_Loaded;
            leaveRequests = new ObservableCollection<LeaveRequestData>();
            dg_requestedLeaves.ItemsSource = leaveRequests;
            dg_requestedLeaves.IsReadOnly = true;
        }

        private void LeaveRequest_Loaded(object sender, RoutedEventArgs e)
        {
            managerId = UserLog.userName;
            LoadLeaveRequests();
        }

        private void LoadLeaveRequests()
        {
            try
            {
                string query = "SELECT Id, EmployeeId, LeaveType, FromDate, ToDate, Reason, Status FROM LeaveRequests";

                using (SqlConnection connection = new SqlConnection("Data Source=Dila99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    leaveRequests.Clear();
                    while (reader.Read())
                    {
                        string requestId = reader["Id"].ToString();
                        string employeeId = reader["EmployeeId"].ToString();
                        string leaveType = reader["LeaveType"].ToString();
                        DateTime fromDate = Convert.ToDateTime(reader["FromDate"]);
                        DateTime toDate = Convert.ToDateTime(reader["ToDate"]);
                        string reason = reader["Reason"].ToString();
                        string status = reader["Status"].ToString();

                        leaveRequests.Add(new LeaveRequestData(requestId, employeeId, leaveType, fromDate, toDate, reason, status));
                    }

                    reader.Close();
                }
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


        private void dg_requestedLeaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_requestedLeaves.SelectedItem != null)
            {
                LeaveRequestData selectedLeave = dg_requestedLeaves.SelectedItem as LeaveRequestData;
                txt_rqId.Text = selectedLeave.RequestId;
                richTxt_Reson.Document.Blocks.Clear();
                richTxt_Reson.Document.Blocks.Add(new Paragraph(new Run(selectedLeave.Reason)));
            }
        }

        private void btn_approve_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeaveRequestStatus("Approved");
        }

        private void btn_disApproved_Click(object sender, RoutedEventArgs e)
        {
            UpdateLeaveRequestStatus("Disapproved");
        }

        // ... (Previous code remains unchanged)

        private void UpdateLeaveRequestStatus(string newStatus)
        {
            try
            {
                if (dg_requestedLeaves.SelectedItem == null)
                {
                    MessageBox.Show("Please select a leave request from the list.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                LeaveRequestData selectedLeave = dg_requestedLeaves.SelectedItem as LeaveRequestData;
                string requestId = selectedLeave.RequestId;

                // Auto-fill the TextBox with the selected RequestId
                txt_rqId.Text = requestId;

                string managerComment = null;
                if (newStatus == "Disapproved")
                {
                    if (string.IsNullOrEmpty(new TextRange(richTxt_Reson.Document.ContentStart, richTxt_Reson.Document.ContentEnd).Text))
                    {
                        MessageBox.Show("Please provide a reason for disapproving the leave request.", "Disapproval Reason Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    managerComment = new TextRange(richTxt_Reson.Document.ContentStart, richTxt_Reson.Document.ContentEnd).Text;
                }

                // Check if managerComment is null or empty and assign an empty string if so
                if (string.IsNullOrEmpty(managerComment))
                {
                    managerComment = string.Empty;
                }

                string updateQuery = "UPDATE LeaveRequests SET Status = @Status, ManagerComment = @ManagerComment WHERE Id = @RequestId";

                using (SqlConnection connection = new SqlConnection("Data Source=Dila99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@Status", newStatus);
                    command.Parameters.AddWithValue("@ManagerComment", managerComment);
                    command.Parameters.AddWithValue("@RequestId", requestId);

                    // ... (Rest of the code remains unchanged)

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Leave request status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Send email notification to the employee
                        SendLeaveApprovalEmail(selectedLeave.EmployeeId, requestId, newStatus);

                        LoadLeaveRequests(); // Reload the leave requests after approval/disapproval
                    }
                    else
                    {
                        MessageBox.Show("Failed to update leave request status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            
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

        // ... (Remaining code remains unchanged)




        private void SendLeaveApprovalEmail(string employeeId, string requestId, string newStatus)
        {
            try
            {
                // Fetch the recipient email from the database based on the employeeId
                string recipientEmail = string.Empty;
                string query = $"SELECT Email FROM Employee WHERE Employee_Id = '{employeeId}'";

                using (SqlConnection connection = new SqlConnection("Data Source=Dila99;Initial Catalog=RRD;Integrated Security=True"))
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

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    MessageBox.Show("Recipient email not found for the employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string senderEmail = "rrdonnellyOfficial@gmail.com"; // Replace with your email address
                string senderPassword = "cvonpgclxinqibnu"; // Replace with your email password

                // Replace the following values with your actual email provider's settings
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;

                using (MailMessage message = new MailMessage(senderEmail, recipientEmail))
                {
                    SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    string subject = newStatus == "Approved" ? "Leave Request Approved" : "Leave Request Disapproved";
                    string body = newStatus == "Approved" ? $"Your leave request with ID {requestId} has been approved." : $"Your leave request with ID {requestId} has been disapproved.";

                    message.Subject = subject;
                    message.Body = body;

                    smtpClient.Send(message);
                }

                MessageBox.Show($"Leave request {newStatus.ToLower()} and email sent to the employee.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // Class representing a leave request
        public class LeaveRequestData
        {
            public string RequestId { get; set; }
            public string EmployeeId { get; set; }
            public string LeaveType { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }

            public LeaveRequestData(string requestId, string employeeId, string leaveType, DateTime fromDate, DateTime toDate, string reason, string status)
            {
                RequestId = requestId;
                EmployeeId = employeeId;
                LeaveType = leaveType;
                FromDate = fromDate;
                ToDate = toDate;
                Reason = reason;
                Status = status;
            }
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_rqId.Clear();
            richTxt_Reson.Document.Blocks.Clear();
        }
    }
}
