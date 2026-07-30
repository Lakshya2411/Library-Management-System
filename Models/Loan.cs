using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Required]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fine Amount ($)")]
        public decimal FineAmount { get; set; } = 0.00m;

        [Required]
        [Display(Name = "Is Returned?")]
        public bool IsReturned { get; set; } = false;

        // Calculate dynamic overdue status or fine
        public bool IsOverdue => !IsReturned && DateTime.Today > DueDate;

        public int OverdueDays
        {
            get
            {
                if (IsReturned)
                {
                    if (ReturnDate.HasValue && ReturnDate.Value > DueDate)
                    {
                        return (ReturnDate.Value - DueDate).Days;
                    }
                }
                else if (DateTime.Today > DueDate)
                {
                    return (DateTime.Today - DueDate).Days;
                }
                return 0;
            }
        }
    }
}
