using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace Kurs_Bank.Models
{
    public class Client
    {
        [Key]
        [StringLength(10)]
        public string PassportNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string Street { get; set; }

        [Required]
        [StringLength(10)]
        public string House { get; set; }

        public virtual Users Users { get; set; }
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
        public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
    }
}