using Kurs_Bank.Data;
using Kurs_Bank.Helpers;
using Kurs_Bank.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Kurs_Bank.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PassportBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(BirthDateBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                string.IsNullOrWhiteSpace(CityBox.Text) ||
                string.IsNullOrWhiteSpace(StreetBox.Text) ||
                string.IsNullOrWhiteSpace(HouseBox.Text) ||
                string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Заполните все обязательные поля");
                return;
            }

            if (!DateTime.TryParseExact(BirthDateBox.Text, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime birthDate))
            {
                ShowError("Неверный формат даты. Используйте дд.мм.гггг");
                return;
            }

            using (var db = new AppDbContext())
            {
                if (db.Clients.Any(c => c.PassportNumber == PassportBox.Text.Trim()))
                {
                    ShowError("Клиент с таким паспортом уже существует");
                    return;
                }

                if (db.Users.Any(u => u.Login == LoginBox.Text.Trim()))
                {
                    ShowError("Такой логин уже занят");
                    return;
                }

                var client = new Client
                {
                    PassportNumber = PassportBox.Text.Trim(),
                    LastName = LastNameBox.Text.Trim(),
                    FirstName = FirstNameBox.Text.Trim(),
                    MiddleName = MiddleNameBox.Text.Trim(),
                    BirthDate = birthDate,
                    Phone = PhoneBox.Text.Trim(),
                    City = CityBox.Text.Trim(),
                    Street = StreetBox.Text.Trim(),
                    House = HouseBox.Text.Trim()
                };

                var user = new Users
                {
                    PassportNumber = PassportBox.Text.Trim(),
                    Login = LoginBox.Text.Trim(),
                    Password = PasswordBox.Password
                };

                db.Clients.Add(client);
                db.Users.Add(user);
                db.SaveChanges();

                SessionManager.CurrentClient = client;
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
        }

        private void GoToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}