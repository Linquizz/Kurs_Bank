using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank.Pages
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
                var accountIds = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Активен")
                    .Select(a => a.AccountID)
                    .ToList();

                var cards = db.Cards
                    .Include("Account")
                    .Where(c => accountIds.Contains(c.AccountID) && c.Status == "Активна")
                    .ToList();

                FromCardCombo.ItemsSource = cards;

                var transactions = db.TransactionAccounts
                    .Include("Transaction")
                    .Where(ta => accountIds.Contains(ta.AccountID))
                    .Select(ta => ta.Transaction)
                    .Distinct()
                    .OrderByDescending(t => t.OperationDate)
                    .ToList();

                TransactionsGrid.ItemsSource = transactions;
            }
        }

        private void ToCardNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cardNumber = ToCardNumberBox.Text.Trim();
            if (cardNumber.Length != 16)
            {
                ReceiverInfoBorder.Visibility = Visibility.Collapsed;
                return;
            }

            using (var db = new AppDbContext())
            {
                var card = db.Cards
                    .Include("Account")
                    .Include("Account.Client")
                    .FirstOrDefault(c => c.CardNumber == cardNumber);

                if (card != null && card.Status == "Активна")
                {
                    ReceiverInfoText.Text =
                        $"{card.Account.Client.LastName} {card.Account.Client.FirstName} {card.Account.Client.MiddleName}";
                    ReceiverInfoBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    ReceiverInfoBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SendMoney_Click(object sender, RoutedEventArgs e)
        {
            if (FromCardCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите карту отправителя", "Ошибка");
                return;
            }
            if (string.IsNullOrWhiteSpace(ToCardNumberBox.Text) || ToCardNumberBox.Text.Trim().Length != 16)
            {
                MessageBox.Show("Введите корректный номер карты получателя (16 цифр)", "Ошибка");
                return;
            }
            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка");
                return;
            }
            if (amount < 1)
            {
                MessageBox.Show("Минимальная сумма перевода: 1 ₽", "Ошибка");
                return;
            }
            if (amount > 500000)
            {
                MessageBox.Show("Максимальная сумма одного перевода: 500 000 ₽", "Ошибка");
                return;
            }

            var fromCard = FromCardCombo.SelectedItem as Card;
            string toCardNumber = ToCardNumberBox.Text.Trim();

            if (fromCard.CardNumber == toCardNumber)
            {
                MessageBox.Show("Нельзя переводить на ту же карту", "Ошибка");
                return;
            }

            using (var db = new AppDbContext())
            {
                var toCard = db.Cards
                    .Include("Account")
                    .FirstOrDefault(c => c.CardNumber == toCardNumber);

                if (toCard == null)
                {
                    MessageBox.Show("Карта получателя не найдена", "Ошибка");
                    return;
                }
                if (toCard.Status == "Заблокирована")
                {
                    MessageBox.Show("Карта получателя заблокирована", "Ошибка");
                    return;
                }
                if (toCard.Account.Status == "Закрыт" || toCard.Account.Status == "Заблокирован")
                {
                    MessageBox.Show("Счёт получателя недоступен", "Ошибка");
                    return;
                }

                var fromAccount = db.Accounts.Find(fromCard.AccountID);
                if (fromAccount.Balance < amount)
                {
                    MessageBox.Show(
                        $"Недостаточно средств.\nДоступно: {fromAccount.Balance:N2} ₽\nНужно: {amount:N2} ₽",
                        "Ошибка");
                    return;
                }

                var toClient = db.Clients.Find(toCard.Account.PassportNumber);
                var confirm = MessageBox.Show(
                    $"Перевести {amount:N2} ₽?\n\n" +
                    $"С карты: {fromCard.FormattedCardNumber}\n" +
                    $"На карту: {toCard.FormattedCardNumber}\n" +
                    $"Получатель: {toClient?.LastName} {toClient?.FirstName}",
                    "Подтверждение перевода",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                fromAccount.Balance -= amount;
                toCard.Account.Balance += amount;

                var transaction = new Transaction
                {
                    Amount = amount,
                    Status = "Выполнена",
                    Type = "Перевод",
                    OperationDate = DateTime.Now,
                    Comment = $"Перевод с карты {fromCard.FormattedCardNumber} на карту {toCard.FormattedCardNumber}"
                };
                db.Transactions.Add(transaction);
                db.SaveChanges();

                db.TransactionAccounts.Add(new Transaction_Account
                {
                    TransactionID = transaction.TransactionID,
                    AccountID = fromAccount.AccountID,
                    Role = "Отправитель"
                });
                db.TransactionAccounts.Add(new Transaction_Account
                {
                    TransactionID = transaction.TransactionID,
                    AccountID = toCard.Account.AccountID,
                    Role = "Получатель"
                });
                db.SaveChanges();

                MessageBox.Show("Перевод выполнен успешно!", "Успех");
                ToCardNumberBox.Text = "";
                AmountBox.Text = "";
                ReceiverInfoBorder.Visibility = Visibility.Collapsed;
                LoadData();
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var passport = SessionManager.CurrentClient.PassportNumber;

            using (var db = new AppDbContext())
            {
                var accountIds = db.Accounts
                    .Where(a => a.PassportNumber == passport)
                    .Select(a => a.AccountID)
                    .ToList();

                var transactions = db.TransactionAccounts
                    .Include("Transaction")
                    .Where(ta => accountIds.Contains(ta.AccountID))
                    .Select(ta => ta.Transaction)
                    .Distinct()
                    .OrderByDescending(t => t.OperationDate)
                    .ToList();

                if (!transactions.Any())
                {
                    MessageBox.Show("Нет транзакций для экспорта", "Информация");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Транзакции_{DateTime.Now:dd-MM-yyyy}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файлы (.xlsx)|*.xlsx"
                };

                if (saveDialog.ShowDialog() != true) return;

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Транзакции");

                    worksheet.Cell(1, 1).Value = "№";
                    worksheet.Cell(1, 2).Value = "Дата и время";
                    worksheet.Cell(1, 3).Value = "Тип";
                    worksheet.Cell(1, 4).Value = "Сумма (₽)";
                    worksheet.Cell(1, 5).Value = "Статус";
                    worksheet.Cell(1, 6).Value = "Комментарий";

                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#5C3FBE");
                    headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    for (int i = 0; i < transactions.Count; i++)
                    {
                        var t = transactions[i];
                        int row = i + 2;

                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = t.OperationDate.ToString("dd.MM.yyyy HH:mm");
                        worksheet.Cell(row, 3).Value = t.Type;
                        worksheet.Cell(row, 4).Value = (double)t.Amount;
                        worksheet.Cell(row, 5).Value = t.Status;
                        worksheet.Cell(row, 6).Value = t.Comment ?? "";

                        if (i % 2 == 0)
                        {
                            worksheet.Row(row).Style.Fill.BackgroundColor =
                                ClosedXML.Excel.XLColor.FromHtml("#F0EAFB");
                        }

                        var statusCell = worksheet.Cell(row, 5);
                        if (t.Status == "Выполнена")
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
                        else if (t.Status == "Отклонена")
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                        else
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Orange;
                    }

                    int lastRow = transactions.Count + 2;
                    worksheet.Cell(lastRow, 3).Value = "ИТОГО:";
                    worksheet.Cell(lastRow, 3).Style.Font.Bold = true;
                    worksheet.Cell(lastRow, 4).Value = (double)transactions.Sum(t => t.Amount);
                    worksheet.Cell(lastRow, 4).Style.Font.Bold = true;
                    worksheet.Cell(lastRow, 4).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#5C3FBE");

                    worksheet.Columns().AdjustToContents();

                    var tableRange = worksheet.Range(1, 1, lastRow, 6);
                    tableRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    tableRange.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#5C3FBE");
                    tableRange.Style.Border.InsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#A78BFA");

                    workbook.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show(
                    $"Экспорт выполнен успешно!\nФайл сохранён: {saveDialog.FileName}",
                    "Успех");
            }
        }
    }
}