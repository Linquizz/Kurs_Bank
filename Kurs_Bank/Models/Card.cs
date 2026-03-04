using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey("AccountID")]
        public virtual Account Account { get; set; }
    }
}