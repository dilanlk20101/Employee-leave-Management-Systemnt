using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Test.Manager
{
    /// <summary>
    /// Interaction logic for DeleteEmployee.xaml
    /// </summary>
    public partial class DeleteEmployee : UserControl
    {
        private ObservableCollection<DeletedEmployee> deletedEmployees;

        public DeleteEmployee()
        {
            InitializeComponent();
            deletedEmployees = new ObservableCollection<DeletedEmployee>();
            dg_delEmp.ItemsSource = deletedEmployees;

            // Load the deleted employee details initially
            LoadDeletedEmployees();
        }

        private void LoadDeletedEmployees()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                {
                    connection.Open();

                    string selectDeletedQuery = "SELECT * FROM DeletedEmployee";
                    using (SqlCommand selectDeletedCommand = new SqlCommand(selectDeletedQuery, connection))
                    {
                        using (SqlDataReader reader = selectDeletedCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string deletedEmpId = reader["Employee_Id"].ToString();
                                string firstName = reader["First_Name"].ToString();
                                string lastName = reader["Last_Name"].ToString();
                                string nic = reader["NIC_Number"].ToString();
                                DateTime deletionDate = Convert.ToDateTime(reader["Deletion_Date"]);

                                DeletedEmployee deletedEmployee = new DeletedEmployee(deletedEmpId, firstName, lastName, nic, deletionDate);
                                deletedEmployees.Add(deletedEmployee);
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

        private void Btn_empDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string empId = txt_emp_id.Text;

                if (string.IsNullOrEmpty(empId))
                {
                    MessageBox.Show("Employee ID cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBoxResult confirmResult = MessageBox.Show("Are you sure you want to delete this employee?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True"))
                    {
                        connection.Open();

                        string selectDeletedQuery = "SELECT * FROM Employee WHERE Employee_Id = @EmpId";
                        using (SqlCommand selectDeletedCommand = new SqlCommand(selectDeletedQuery, connection))
                        {
                            selectDeletedCommand.Parameters.AddWithValue("@EmpId", empId);
                            using (SqlDataReader reader = selectDeletedCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string deletedEmpId = reader["Employee_Id"].ToString();
                                    string firstName = reader["First_Name"].ToString();
                                    string lastName = reader["Last_Name"].ToString();
                                    string nic = reader["NIC_Number"].ToString();
                                    DateTime deletionDate = DateTime.Now;

                                    reader.Close();

                                    string insertDeletedQuery = "INSERT INTO DeletedEmployee (Employee_Id, First_Name, Last_Name, NIC_Number, Deletion_Date) VALUES (@DeletedEmpId, @FirstName, @LastName, @NIC, @DeletionDate)";
                                    using (SqlCommand insertDeletedCommand = new SqlCommand(insertDeletedQuery, connection))
                                    {
                                        insertDeletedCommand.Parameters.AddWithValue("@DeletedEmpId", deletedEmpId);
                                        insertDeletedCommand.Parameters.AddWithValue("@FirstName", firstName);
                                        insertDeletedCommand.Parameters.AddWithValue("@LastName", lastName);
                                        insertDeletedCommand.Parameters.AddWithValue("@NIC", nic);
                                        insertDeletedCommand.Parameters.AddWithValue("@DeletionDate", deletionDate);
                                        insertDeletedCommand.ExecuteNonQuery();

                                        DeletedEmployee deletedEmployee = new DeletedEmployee(deletedEmpId, firstName, lastName, nic, deletionDate);
                                        deletedEmployees.Add(deletedEmployee);

                                        string deleteQuery = "DELETE FROM Employee WHERE Employee_Id = @EmpId";
                                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                                        {
                                            deleteCommand.Parameters.AddWithValue("@EmpId", empId);
                                            int rowsAffected = deleteCommand.ExecuteNonQuery();

                                            if (rowsAffected > 0)
                                            {
                                                MessageBox.Show("Employee deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                            }
                                            else
                                            {
                                                MessageBox.Show("Error occurred while deleting the employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
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

        // Class representing a deleted employee
        public class DeletedEmployee
        {
            public string EmployeeId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NIC { get; set; }
            public DateTime DeletionDate { get; set; }

            public DeletedEmployee(string employeeId, string firstName, string lastName, string nic, DateTime deletionDate)
            {
                EmployeeId = employeeId;
                FirstName = firstName;
                LastName = lastName;
                NIC = nic;
                DeletionDate = deletionDate;
            }
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            txt_emp_id.Clear();
        }
    }
}