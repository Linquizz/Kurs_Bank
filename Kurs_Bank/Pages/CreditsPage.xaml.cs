using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank
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
                var accounts = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .ToList();
                CreditAccountCombo.ItemsSource = accounts;

                var credits = db.Credits
                    .Include("CreditPayments")
                    .Where(c => c.PassportNumber == passport)
                    .ToList();

                var creditViewModels = credits.Select(c => new CreditViewModel
                {
                    CreditID = c.CreditID,
                    Amount = c.Amount,
                    InterestRate = c.InterestRate,
                    TermMonths = c.TermMonths,
                    Status = c.Status,
                    AccountID = c.AccountID,
                    MonthlyPayment = Math.Round(
                        c.Amount * (1 + c.InterestRate / 100) / c.TermMonths, 2),
                    CreditPayments = c.CreditPayments.ToList()
                }).ToList();

                CreditsList.ItemsSource = creditViewModels;
            }
        }

        private decimal CalculateRate(decimal amount, int term)
        {
            decimal rate = 20.0m;
            if (amount >= 100000) rate = 16.0m;
            if (amount >= 300000) rate = 14.0m;
            if (amount >= 500000) rate = 12.0m;
            if (amount >= 1000000) rate = 10.0m;
            if (term >= 24) rate -= 1.0m;
            if (term >= 36) rate -= 1.0m;
            return rate;
        }

        private void ApplyCredit_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(CreditAmountBox.Text, out decimal amount))
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка");
                return;
            }
            if (amount < 10000)
            {
                MessageBox.Show("Минимальная сумма кредита: 10 000 ₽", "Ошибка");
                return;
            }
            if (amount > 5000000)
            {
                MessageBox.Show("Максимальная сумма кредита: 5 000 000 ₽", "Ошибка");
                return;
            }
            if (!int.TryParse(CreditTermBox.Text, out int term))
            {
                MessageBox.Show("Введите корректный срок", "Ошибка");
                return;
            }

            int minTerm = amount <= 100000 ? 3 : amount <= 500000 ? 6 : 12;
            int maxTerm = amount <= 100000 ? 24 : amount <= 500000 ? 60 : 84;

            if (term < minTerm)
            {
                MessageBox.Show($"Минимальный срок для данной суммы: {minTerm} месяцев", "Ошибка");
                return;
            }
            if (term > maxTerm)
            {
                MessageBox.Show($"Максимальный срок для данной суммы: {maxTerm} месяцев", "Ошибка");
                return;
            }
            if (CreditAccountCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите счёт для зачисления", "Ошибка");
                return;
            }

            var account = CreditAccountCombo.SelectedItem as Account;
            decimal rate = CalculateRate(amount, term);
            decimal monthlyPayment = Math.Round(amount * (1 + rate / 100) / term, 2);

            var confirm = MessageBox.Show(
                $"Сумма кредита: {amount:N2} ₽\n" +
                $"Срок: {term} мес.\n" +
                $"Процентная ставка: {rate}%\n" +
                $"Ежемесячный платёж: {monthlyPayment:N2} ₽\n\n" +
                $"Подтвердить оформление?",
                "Подтверждение", MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes) return;

            using (var db = new AppDbContext())
            {
                var credit = new Credit
                {
                    PassportNumber = SessionManager.CurrentClient.PassportNumber,
                    AccountID = account.AccountID,
                    Amount = amount,
                    Status = "Активен",
                    TermMonths = term,
                    InterestRate = rate,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(term)
                };
                db.Credits.Add(credit);
                db.SaveChanges();

                var dbAccount = db.Accounts.Find(account.AccountID);
                dbAccount.Balance += amount;

                for (int i = 1; i <= term; i++)
                {
                    db.CreditPayments.Add(new CreditPayment
                    {
                        CreditID = credit.CreditID,
                        Amount = monthlyPayment,
                        PlannedDate = DateTime.Now.AddMonths(i),
                        ActualDate = null,
                        Status = "Ожидает"
                    });
                }
                db.SaveChanges();

                MessageBox.Show(
                    $"Кредит оформлен!\nНа счёт зачислено: {amount:N2} ₽\nЕжемесячный платёж: {monthlyPayment:N2} ₽",
                    "Успех");
                LoadData();
            }
        }

        private void PayInstallment_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int paymentId)) return;

            using (var db = new AppDbContext())
            {
                var payment = db.CreditPayments
                    .Include("Credit")
                    .FirstOrDefault(p => p.PaymentID == paymentId);

                if (payment == null || payment.Status != "Ожидает")
                {
                    MessageBox.Show("Платёж уже оплачен или не найден", "Ошибка");
                    return;
                }

                var account = db.Accounts.Find(payment.Credit.AccountID);
                if (account == null)
                {
                    MessageBox.Show("Счёт не найден", "Ошибка");
                    return;
                }
                if (account.Balance < payment.Amount)
                {
                    MessageBox.Show(
                        $"Недостаточно средств.\nНужно: {payment.Amount:N2} ₽\nНа счёте: {account.Balance:N2} ₽",
                        "Ошибка");
                    return;
                }

                account.Balance -= payment.Amount;
                payment.Status = "Оплачен";
                payment.ActualDate = DateTime.Now;

                var allPaid = db.CreditPayments
                    .Where(p => p.CreditID == payment.CreditID && p.PaymentID != paymentId)
                    .All(p => p.Status == "Оплачен");

                if (allPaid)
                {
                    var credit = db.Credits.Find(payment.CreditID);
                    if (credit != null) credit.Status = "Погашен";
                }

                db.SaveChanges();
                MessageBox.Show($"Платёж {payment.Amount:N2} ₽ выполнен!", "Успех");
                LoadData();
            }
        }
    }

    public class CreditViewModel
    {
        public int CreditID { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public string Status { get; set; }
        public int AccountID { get; set; }
        public decimal MonthlyPayment { get; set; }
        public List<CreditPayment> CreditPayments { get; set; }
    }
}