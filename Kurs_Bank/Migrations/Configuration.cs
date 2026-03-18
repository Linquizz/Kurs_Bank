using Kurs_Bank.Models;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Kurs_Bank.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Kurs_Bank.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Kurs_Bank.Data.AppDbContext context)
        {
            if (!context.Clients.Any(c => c.PassportNumber == "0000000000"))
            {
                var adminClient = new Client
                {
                    PassportNumber = "0000000000",
                    LastName = "Администратор",
                    FirstName = "Admin",
                    MiddleName = "",
                    BirthDate = new DateTime(1990, 1, 1),
                    Phone = "+70000000000",
                    City = "Москва",
                    Street = "Административная",
                    House = "1"
                };
                context.Clients.Add(adminClient);
                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.Login == "admin"))
            {
                var adminUser = new Users
                {
                    PassportNumber = "0000000000",
                    Login = "admin",
                    Password = "12345"
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }
}