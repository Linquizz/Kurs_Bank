using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Views.Pages
{
    public partial class CardsPage : Page
    {
        public CardsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var passport = SessionManager.CurrentClient.PassportNumber;
            using (var db = new AppDbContext())
            {
                var accountIds = db.Accounts
                    .Where(a => a.PassportNumber == passport)
                    .Select(a => a.AccountID)
                    .ToList();

                CardsList.ItemsSource = db.Cards
                    .Where(c => accountIds.Contains(c.AccountID))
                    .ToList();

                CardAccountCombo.ItemsSource = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .ToList();
                CardAccountCombo.DisplayMemberPath = "AccountID";
            }
        }

        private void IssueCard_Click(object sender, RoutedEventArgs e)
        {
            if (CardAccountCombo.SelectedItem == null || CardTypeCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите счёт и тип карты", "Ошибка");
                return;
            }

            var account = CardAccountCombo.SelectedItem as Account;
            var cardType = (CardTypeCombo.SelectedItem as ComboBoxItem).Content.ToString();
            var rnd = new Random();
            string cardNumber = string.Concat(Enumerable.Range(0, 16).Select(_ => rnd.Next(0, 10).ToString()));

            using (var db = new AppDbContext())
            {
                var card = new Card
                {
                    CardNumber = cardNumber,
                    AccountID = account.AccountID,
                    CardType = cardType,
                    IssueDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(4),
                    Status = "Активна"
                };
                db.Cards.Add(card);
                db.SaveChanges();
                MessageBox.Show("Карта выпущена!", "Успех");
                LoadData();
            }
        }
    }
}