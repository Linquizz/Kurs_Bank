using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kurs_Bank.Models
{
    public class Card
    {
        [Key]
        [StringLength(16)]
        public string CardNumber { get; set; }

        [Required]
        public int AccountID { get; set; }

        [Required]
        [StringLength(20)]
        public string CardType { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public virtual Account Account { get; set; }

        [NotMapped]
        public string FormattedCardNumber
        {
            get
            {
                if (string.IsNullOrEmpty(CardNumber) || CardNumber.Length != 16)
                    return CardNumber;
                return CardNumber.Substring(0, 4) + " " +
                       CardNumber.Substring(4, 4) + " " +
                       CardNumber.Substring(8, 4) + " " +
                       CardNumber.Substring(12, 4);
            }
            private set { }
        }
        [NotMapped]
        public bool IsActive => Status == "Активна";

        [NotMapped]
        public bool IsBlocked => Status == "Заблокирована";
    }
}