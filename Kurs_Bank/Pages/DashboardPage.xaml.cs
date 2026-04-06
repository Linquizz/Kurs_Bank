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

        private void CloseAccount_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int accountId = (int)btn.Tag;

            var result = MessageBox.Show(
                "Вы уверены что хотите закрыть счёт?", "Закрытие счёта",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    var account = db.Accounts.Find(accountId);
                    if (account.Balance > 0)
                    {
                        MessageBox.Show("Нельзя закрыть счёт с ненулевым балансом", "Ошибка");
                        return;
                    }
                    account.Status = "Закрыт";
                    account.CloseDate = DateTime.Now;
                    db.SaveChanges();
                    LoadData();
                }
            }
        }

        private void ExportAccounts_Click(object sender, RoutedEventArgs e)
        {
            var passport = SessionManager.CurrentClient.PassportNumber;

            using (var db = new AppDbContext())
            {
                var accounts = db.Accounts
                    .Where(a => a.PassportNumber == passport)
                    .ToList();

                if (!accounts.Any())
                {
                    MessageBox.Show("Нет счетов для экспорта", "Информация");
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Счета_{DateTime.Now:dd-MM-yyyy}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файлы (.xlsx)|*.xlsx"
                };

                if (saveDialog.ShowDialog() != true) return;

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Счета");

                    worksheet.Cell(1, 1).Value = "№";
                    worksheet.Cell(1, 2).Value = "ID счёта";
                    worksheet.Cell(1, 3).Value = "Тип счёта";
                    worksheet.Cell(1, 4).Value = "Баланс (₽)";
                    worksheet.Cell(1, 5).Value = "Статус";
                    worksheet.Cell(1, 6).Value = "Дата открытия";
                    worksheet.Cell(1, 7).Value = "Дата закрытия";

                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#5C3FBE");
                    headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    for (int i = 0; i < accounts.Count; i++)
                    {
                        var a = accounts[i];
                        int row = i + 2;

                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = a.AccountID;
                        worksheet.Cell(row, 3).Value = a.AccountType;
                        worksheet.Cell(row, 4).Value = (double)a.Balance;
                        worksheet.Cell(row, 5).Value = a.Status;
                        worksheet.Cell(row, 6).Value = a.OpenDate.ToString("dd.MM.yyyy");
                        worksheet.Cell(row, 7).Value = a.CloseDate.HasValue
                            ? a.CloseDate.Value.ToString("dd.MM.yyyy")
                            : "—";

                        if (i % 2 == 0)
                            worksheet.Row(row).Style.Fill.BackgroundColor =
                                ClosedXML.Excel.XLColor.FromHtml("#F0EAFB");

                        var statusCell = worksheet.Cell(row, 5);
                        if (a.Status == "Активен")
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
                        else if (a.Status == "Заблокирован")
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Orange;
                        else if (a.Status == "Закрыт")
                            statusCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                    }

                    var tableRange = worksheet.Range(1, 1, accounts.Count + 1, 7);
                    tableRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    tableRange.Style.Border.OutsideBorderColor =
                        ClosedXML.Excel.XLColor.FromHtml("#5C3FBE");
                    tableRange.Style.Border.InsideBorderColor =
                        ClosedXML.Excel.XLColor.FromHtml("#A78BFA");

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show(
                    $"Экспорт выполнен!\nФайл сохранён: {saveDialog.FileName}",
                    "Успех");
            }
        }
    }

}