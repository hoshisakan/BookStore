using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoshiBook.Models
{
    [Table("CoverTypes", Schema = "bookstore")]
    public class CoverType
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Cover Type")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
    }
}