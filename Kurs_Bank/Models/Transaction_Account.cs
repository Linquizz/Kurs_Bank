using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class Transaction_Account
    {
        [Key]
        public int TransactionAccountID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        [Required]
        public int AccountID { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        [ForeignKey("TransactionID")]
        public virtual Transaction Transaction { get; set; }

        [ForeignKey("AccountID")]
        public virtual Account Account { get; set; }
    }
}