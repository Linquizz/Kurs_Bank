using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        [StringLength(30)]
        public string Type { get; set; }

        [StringLength(255)]
        public string Comment { get; set; }

        [Required]
        public DateTime OperationDate { get; set; }

        public virtual ICollection<Transaction_Account> TransactionAccounts { get; set; } = new List<Transaction_Account>();
    }
}