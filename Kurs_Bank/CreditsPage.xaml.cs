using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Views.Pages
{
    public partial class CreditsPage : Page
    {
        public CreditsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var passport = SessionManager.CurrentClient.PassportNumber;
            using (var db = new AppDbContext())
            {
                CreditsList.ItemsSource = db.Credits
                    .Where(c => c.PassportNumber == passport)
                    .ToList();

                CreditAccountCombo.ItemsSource = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .ToList();
                CreditAccountCombo.DisplayMemberPath = "AccountID";
            }
        }

        private void ApplyCredit_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(CreditAmountBox.Text, out decimal amount) || amount <= 0 ||
                !int.TryParse(CreditTermBox.Text, out int term) || term <= 0 ||
                CreditAccountCombo.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля корректно", "Ошибка");
                return;
            }

            var account = CreditAccountCombo.SelectedItem as Account;
            using (var db = new AppDbContext())
            {
                var credit = new Credit
                {
                    PassportNumber = SessionManager.CurrentClient.PassportNumber,
                    AccountID = account.AccountID,
                    Amount = amount,
                    Status = "Активен",
                    TermMonths = term,
                    InterestRate = 12.5m,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(term)
                };
                db.Credits.Add(credit);
                db.SaveChanges();
                MessageBox.Show("Кредит оформлен!", "Успех");
                LoadData();
            }
        }

        private void PayCredit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int creditId = (int)btn.Tag;

            using (var db = new AppDbContext())
            {
                var credit = db.Credits.Find(creditId);
                var account = db.Accounts.Find(credit.AccountID);
                decimal monthlyPayment = Math.Round(credit.Amount / credit.TermMonths, 2);

                if (account.Balance < monthlyPayment)
                {
                    MessageBox.Show("Недостаточно средств на счёте", "Ошибка");
                    return;
                }

                account.Balance -= monthlyPayment;
                db.CreditPayments.Add(new CreditPayment
                {
                    CreditID = creditId,
                    Amount = monthlyPayment,
                    PlannedDate = DateTime.Now,
                    ActualDate = DateTime.Now,
                    Status = "Оплачен"
                });
                db.SaveChanges();
                MessageBox.Show($"Платёж {monthlyPayment:N2} ₽ выполнен!", "Успех");
                LoadData();
            }
        }
    }
}