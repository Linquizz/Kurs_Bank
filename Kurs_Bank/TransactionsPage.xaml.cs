using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Views.Pages
{
    public partial class TransactionsPage : Page
    {
        public TransactionsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var passport = SessionManager.CurrentClient.PassportNumber;
            using (var db = new AppDbContext())
            {
                var accounts = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .ToList();
                FromAccountCombo.ItemsSource = accounts;
                FromAccountCombo.DisplayMemberPath = "AccountID";

                var myAccountIds = accounts.Select(a => a.AccountID).ToList();
                var transactions = db.TransactionAccounts
                    .Include("Transaction")
                    .Where(ta => myAccountIds.Contains(ta.AccountID))
                    .Select(ta => ta.Transaction)
                    .Distinct()
                    .OrderByDescending(t => t.OperationDate)
                    .ToList();
                TransactionsGrid.ItemsSource = transactions;
            }
        }

        private void SendMoney_Click(object sender, RoutedEventArgs e)
        {
            if (FromAccountCombo.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ToAccountBox.Text) ||
                string.IsNullOrWhiteSpace(AmountBox.Text))
            {
                MessageBox.Show("Заполните все поля", "Ошибка");
                return;
            }

            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка");
                return;
            }

            using (var db = new AppDbContext())
            {
                var fromAccount = FromAccountCombo.SelectedItem as Account;
                var fromDb = db.Accounts.Find(fromAccount.AccountID);

                if (fromDb.Balance < amount)
                {
                    MessageBox.Show("Недостаточно средств", "Ошибка");
                    return;
                }

                if (!int.TryParse(ToAccountBox.Text, out int toAccountId))
                {
                    MessageBox.Show("Неверный номер счёта получателя", "Ошибка");
                    return;
                }

                var toAccount = db.Accounts.Find(toAccountId);
                if (toAccount == null)
                {
                    MessageBox.Show("Счёт получателя не найден", "Ошибка");
                    return;
                }

                fromDb.Balance -= amount;
                toAccount.Balance += amount;

                var transaction = new Transaction
                {
                    Amount = amount,
                    Status = "Выполнена",
                    Type = "Перевод",
                    OperationDate = DateTime.Now,
                    Comment = $"Перевод на счёт {toAccountId}"
                };
                db.Transactions.Add(transaction);
                db.SaveChanges();

                db.TransactionAccounts.Add(new Transaction_Account
                {
                    TransactionID = transaction.TransactionID,
                    AccountID = fromDb.AccountID,
                    Role = "Отправитель"
                });
                db.TransactionAccounts.Add(new Transaction_Account
                {
                    TransactionID = transaction.TransactionID,
                    AccountID = toAccount.AccountID,
                    Role = "Получатель"
                });
                db.SaveChanges();

                MessageBox.Show("Перевод выполнен успешно!", "Успех");
                LoadData();
            }
        }
    }
}
