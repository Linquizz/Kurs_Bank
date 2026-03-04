using Kurs_Bank.Models;

namespace Kurs_Bank.Helpers
{
    public static class SessionManager
    {
        public static Client CurrentClient { get; set; }
    }
}