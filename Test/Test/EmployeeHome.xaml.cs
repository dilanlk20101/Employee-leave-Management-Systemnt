using System;
using System.Collections.Generic;
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

namespace Test
{
    /// <summary>
    /// Interaction logic for EmployeeHome.xaml
    /// </summary>
    public partial class EmployeeHome : Window
    {
        private UserControl activeUserControl;
        public EmployeeHome()
        {
            InitializeComponent();
            Customize();
            ShowUserControl(uc_Dashboard);
            Loaded += EmployeeHome_Load; // Subscribe to the Loaded event
        }
        private void ShowUserControl(UserControl userControl)
        {
            if (activeUserControl != null)
                activeUserControl.Visibility = Visibility.Collapsed;

            activeUserControl = userControl;
            activeUserControl.Visibility = Visibility.Visible;
        }
        private void Customize()
        {
            ReportsubCanvas.Visibility = Visibility.Collapsed;
        }
        private void Btn_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowUserControl(uc_Dashboard);
        }

        private void Btn_Report_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsubCanvas.Visibility == Visibility.Visible)
                ReportsubCanvas.Visibility = Visibility.Collapsed;
            else
                ReportsubCanvas.Visibility = Visibility.Visible;
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void EmployeeHome_Load(object sender, RoutedEventArgs e)
        {
            lbl_userName.Content = UserLog.userName;
        }

        private void Btn_Close_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Btn_min_Click_1(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Minimized;
            else
                WindowState = WindowState.Normal;
        }
    }
}
