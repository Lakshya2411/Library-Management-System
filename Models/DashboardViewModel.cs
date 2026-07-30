using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveLoansCount { get; set; }
        public int OverdueLoansCount { get; set; }
        public decimal TotalFinesCollected { get; set; }
        public decimal TotalPendingFines { get; set; }

        public List<Book> PopularBooks { get; set; } = new();
        public List<Loan> OverdueLoans { get; set; } = new();
        public List<Loan> RecentLoans { get; set; } = new();
        
        // Data for Chart.js representation
        public List<string> GenreLabels { get; set; } = new();
        public List<int> GenreCounts { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyBorrowCounts { get; set; } = new();
    }
}
