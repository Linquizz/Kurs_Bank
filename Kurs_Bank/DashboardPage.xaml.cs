using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Views.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var client = SessionManager.CurrentClient;
            GreetingText.Text = $"Привет, {client.FirstName} {client.LastName}!";

            using (var db = new AppDbContext())
            {
                var accounts = db.Accounts
                    .Where(a => a.PassportNumber == client.PassportNumber)
                    .ToList();
                AccountsList.ItemsSource = accounts;
            }
        }

        private void OpenAccount_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Открыть текущий счёт?", "Новый счёт",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    var account = new Account
                    {
                        PassportNumber = SessionManager.CurrentClient.PassportNumber,
                        AccountType = "Текущий",
                        Balance = 0,
                        OpenDate = DateTime.Now,
                        Status = "Активен"
                    };
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    LoadData();
                }
            }
        }
    }
}