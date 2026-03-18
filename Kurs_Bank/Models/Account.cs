using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountID { get; set; }

        [Required]
        [StringLength(10)]
        public string PassportNumber { get; set; }

        [Required]
        [StringLength(30)]
        public string AccountType { get; set; }

        [Required]
        public decimal Balance { get; set; }

        [Required]
        public DateTime OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public virtual Client Client { get; set; }
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
        public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
        public virtual ICollection<Transaction_Account> TransactionAccounts { get; set; } = new List<Transaction_Account>();

        [NotMapped]
        public string AccountInfo
        {
            get { return $"Счёт №{AccountID} — {Balance:N2} ₽ ({AccountType})"; }
        }
    }
}