using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class CreditPayment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int CreditID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PlannedDate { get; set; }

        public DateTime? ActualDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [ForeignKey("CreditID")]
        public virtual Credit Credit { get; set; }
    }
}