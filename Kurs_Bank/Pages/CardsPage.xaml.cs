using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Pages
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
                    .Include("Account")
                    .Where(c => accountIds.Contains(c.AccountID))
                    .ToList();

                CardAccountCombo.ItemsSource = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .ToList();
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

            using (var db = new AppDbContext())
            {
                int existingCards = db.Cards.Count(c => c.AccountID == account.AccountID);
                if (existingCards >= 3)
                {
                    MessageBox.Show("К одному счёту можно привязать не более 3 карт", "Ошибка");
                    return;
                }

                var rnd = new Random();
                string cardNumber;
                do
                {
                    cardNumber = string.Concat(Enumerable.Range(0, 16)
                        .Select(_ => rnd.Next(0, 10).ToString()));
                } while (db.Cards.Any(c => c.CardNumber == cardNumber));

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
                MessageBox.Show($"Карта выпущена!\nНомер: {card.FormattedCardNumber}", "Успех");
                LoadData();
            }
        }

        private void BlockCard_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            var result = MessageBox.Show(
                "Заблокировать карту?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                ChangeCardStatus(btn.Tag.ToString(), "Заблокирована");
        }

        private void ActivateCard_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            ChangeCardStatus(btn.Tag.ToString(), "Активна");
        }

        private void ChangeCardStatus(string cardNumber, string status)
        {
            using (var db = new AppDbContext())
            {
                var card = db.Cards.Find(cardNumber);
                if (card != null)
                {
                    card.Status = status;
                    db.SaveChanges();
                    MessageBox.Show($"Статус карты изменён: {status}");
                    LoadData();
                }
            }
        }

        private void DeleteCard_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;

            string cardNumber = btn.Tag.ToString();

            var result = MessageBox.Show(
                "Удалить заблокированную карту навсегда?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new AppDbContext())
            {
                var card = db.Cards.Find(cardNumber);
                if (card == null) return;

                if (card.Status != "Заблокирована")
                {
                    MessageBox.Show("Можно удалять только заблокированные карты", "Ошибка");
                    return;
                }

                db.Cards.Remove(card);
                db.SaveChanges();
                MessageBox.Show("Карта удалена", "Успех");
                LoadData();
            }
        }
    }
}