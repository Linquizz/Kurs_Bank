using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs_Bank
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var passport = SessionManager.CurrentClient.PassportNumber;

            using (var db = new AppDbContext())
            {
                var client = db.Clients
                    .Include("Users")
                    .FirstOrDefault(c => c.PassportNumber == passport);

                if (client == null) return;

                AvatarText.Text = client.FirstName.Substring(0, 1).ToUpper();
                FullNameText.Text = $"{client.LastName} {client.FirstName} {client.MiddleName}";
                PassportText.Text = $"Паспорт: {client.PassportNumber}";
                PhoneText.Text = $"Телефон: {client.Phone}";
                BirthDateText.Text = client.BirthDate.ToString("dd.MM.yyyy");
                CityText.Text = client.City;
                StreetText.Text = client.Street;
                HouseText.Text = client.House;
                LoginText.Text = client.Users?.Login ?? "—";

                var closedAccounts = db.Accounts
                    .Where(a => a.PassportNumber == passport && a.Status == "Закрыт")
                    .ToList();

                if (closedAccounts.Any())
                {
                    ClosedAccountsList.ItemsSource = closedAccounts;
                    NoClosedAccountsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoClosedAccountsText.Visibility = Visibility.Visible;
                }
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = OldPasswordBox.Password;
            string newPass = NewPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Заполните оба поля", "Ошибка");
                return;
            }
            if (newPass.Length < 4)
            {
                MessageBox.Show("Новый пароль должен быть не менее 4 символов", "Ошибка");
                return;
            }

            var passport = SessionManager.CurrentClient.PassportNumber;
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.PassportNumber == passport);
                if (user == null) return;

                if (user.Password != oldPass)
                {
                    MessageBox.Show("Неверный текущий пароль", "Ошибка");
                    return;
                }

                user.Password = newPass;
                db.SaveChanges();
                MessageBox.Show("Пароль успешно изменён!", "Успех");
                OldPasswordBox.Clear();
                NewPasswordBox.Clear();
            }
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int accountId = (int)btn.Tag;

            var result = MessageBox.Show(
                "Удалить закрытый счёт навсегда?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new AppDbContext())
            {
                var account = db.Accounts.Find(accountId);
                if (account == null) return;

                var cards = db.Cards.Where(c => c.AccountID == accountId).ToList();
                db.Cards.RemoveRange(cards);

                var taIds = db.TransactionAccounts
                    .Where(ta => ta.AccountID == accountId)
                    .ToList();
                db.TransactionAccounts.RemoveRange(taIds);

                db.Accounts.Remove(account);
                db.SaveChanges();
                MessageBox.Show("Счёт удалён", "Успех");
                LoadData();
            }
        }
    }
}