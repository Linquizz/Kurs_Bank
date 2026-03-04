using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class Credit
    {
        [Key]
        public int CreditID { get; set; }

        [Required]
        [StringLength(10)]
        public string PassportNumber { get; set; }

        [Required]
        public int AccountID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        public int TermMonths { get; set; }

        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [ForeignKey("PassportNumber")]
        public virtual Client Client { get; set; }

        [ForeignKey("AccountID")]
        public virtual Account Account { get; set; }

        public virtual ICollection<CreditPayment> CreditPayments { get; set; } = new List<CreditPayment>();
    }
}