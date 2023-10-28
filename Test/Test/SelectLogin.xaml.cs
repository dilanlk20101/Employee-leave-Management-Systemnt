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
    /// Interaction logic for SelectLogin.xaml
    /// </summary>
    public partial class SelectLogin : Window
    {
        public SelectLogin()
        {
            InitializeComponent();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_mangerLogin_Click(object sender, RoutedEventArgs e)
        {
            Login mLog = new Login();
            this.Close();
            mLog.Show();
        }

        private void btn_EmployeeLogin_Click(object sender, RoutedEventArgs e)
        {
            EmployeeLogin employeeLogin = new EmployeeLogin();
            this.Close();
            employeeLogin.Show();
        }
    }
}
