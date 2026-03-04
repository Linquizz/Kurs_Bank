using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Bank.Models
{
    public class Users
    {
        [Key]
        [StringLength(10)]
        [ForeignKey("Client")]
        public string PassportNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string Login { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        public virtual Client Client { get; set; }
    }
}