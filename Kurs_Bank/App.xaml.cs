using Kurs_Bank.Data;
using System.Data.Entity.Migrations;
using System.Windows;

namespace Kurs_Bank
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = new Migrations.Configuration();
            var migrator = new DbMigrator(config);
            migrator.Update();
        }
    }
}