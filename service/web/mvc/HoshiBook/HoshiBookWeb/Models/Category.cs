using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoshiBookWeb.Models;

[Table("Categories", Schema = "bookstore")]
public class Category {
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [DisplayName("Display Order")]
    [Range(1, 100, ErrorMessage = "Display Order muse be between 1 to 100 only!")]
    public int DisplayOrder { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
}