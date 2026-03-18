using Kurs_Bank.Views.Pages;
using System.Windows;
using Kurs_Bank.Pages;

namespace Kurs_Bank.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new DashboardPage());
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            switch (btn.Tag.ToString())
            {
                case "Dashboard": MainFrame.Navigate(new DashboardPage()); break;
                case "Profile": MainFrame.Navigate(new ProfilePage()); break;
                case "Transactions": MainFrame.Navigate(new TransactionsPage()); break;
                case "Credits": MainFrame.Navigate(new CreditsPage()); break;
                case "Cards": MainFrame.Navigate(new CardsPage()); break;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Helpers.SessionManager.CurrentClient = null;
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
