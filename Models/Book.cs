using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(50)]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total Copies is required.")]
        [Range(0, 1000, ErrorMessage = "Total copies must be between 0 and 1000.")]
        [Display(Name = "Total Copies")]
        public int TotalCopies { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Available copies must be between 0 and 1000.")]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        [Range(1000, 2100, ErrorMessage = "Publish year must be a valid 4-digit year.")]
        [Display(Name = "Publish Year")]
        public int PublishYear { get; set; }

        [StringLength(150)]
        public string Publisher { get; set; } = string.Empty;
    }
}
