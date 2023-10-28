using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl activeUserControl;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Customize();
            ShowUserControl(uc_Dashboard);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lbl_userName.Content = UserLog.userName.ToString();
        }

        private void Customize()
        {
            EmployeesubCanvas.Visibility = Visibility.Collapsed;
            LeavesubCanvas.Visibility = Visibility.Collapsed;
            ReportsubCanvas.Visibility = Visibility.Collapsed;
        }

        private void ShowUserControl(UserControl userControl)
        {
            if (activeUserControl != null)
                activeUserControl.Visibility = Visibility.Collapsed;

            activeUserControl = userControl;
            activeUserControl.Visibility = Visibility.Visible;
        }

        private void Btn_Employee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesubCanvas.Visibility == Visibility.Visible)
                EmployeesubCanvas.Visibility = Visibility.Collapsed;
            else
                EmployeesubCanvas.Visibility = Visibility.Visible;
        }
        private void Btn_Report_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsubCanvas.Visibility == Visibility.Visible)
                ReportsubCanvas.Visibility = Visibility.Collapsed;
            else
                ReportsubCanvas.Visibility = Visibility.Visible;
        }

        private void Btn_addEmployee_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_addEmployee1);
        }

        private void Btn_min_Click_1(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Minimized;
            else
                WindowState = WindowState.Normal;
        }
        private void Btn_Close_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Btn_UpdateEmployee_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_updateEmployee);
        }

        private void Btn_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_Dashboard);
        }

        private void Btn_DeleteEmployee_Click_1(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_DeleteEmployee);
        }
        private void Btn_LeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_LeaveRequest);
        }

        private void Btn_EmployeeReport_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_EmployeeReport);
        }
        private void Btn_LeaveReport_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_LeaveReport);
        }

        private void Btn_AttendanceReport_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_AttendenceReport);
        }
        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            lbl_userName.Content = UserLog.userName.ToString();
        }

        private void Btn_Leave_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_LeaveRequest);
        }
    }
}
