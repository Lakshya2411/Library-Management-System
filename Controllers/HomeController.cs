using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(LibraryDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;

                var totalBooks = await _context.Books.CountAsync();
                var totalCopies = await _context.Books.SumAsync(b => b.TotalCopies);
                var availableCopies = await _context.Books.SumAsync(b => b.AvailableCopies);
                var totalMembers = await _context.Members.CountAsync();

                var activeLoans = await _context.Loans
                    .Where(l => !l.IsReturned)
                    .Include(l => l.Book)
                    .Include(l => l.Member)
                    .ToListAsync();

                var activeLoansCount = activeLoans.Count;
                var overdueLoans = activeLoans.Where(l => today > l.DueDate).ToList();
                var overdueLoansCount = overdueLoans.Count;

                // Fines collected (returned loans)
                var totalFinesCollected = await _context.Loans
                    .Where(l => l.IsReturned)
                    .SumAsync(l => l.FineAmount);

                // Pending fines (unreturned, overdue: $1.00 per day)
                var totalPendingFines = activeLoans
                    .Where(l => today > l.DueDate)
                    .Sum(l => (decimal)(today - l.DueDate).Days * 1.00m);

                var recentLoans = await _context.Loans
                    .OrderByDescending(l => l.IssueDate)
                    .Take(5)
                    .Include(l => l.Book)
                    .Include(l => l.Member)
                    .ToListAsync();

                // Popular books (by number of times borrowed)
                var popularBooksData = await _context.Loans
                    .GroupBy(l => l.BookId)
                    .OrderByDescending(g => g.Count())
                    .Take(4)
                    .Select(g => g.Key)
                    .ToListAsync();

                var popularBooks = await _context.Books
                    .Where(b => popularBooksData.Contains(b.Id))
                    .ToListAsync();

                // If not enough loans to determine popular, just take first 4 books
                if (popularBooks.Count < 4)
                {
                    popularBooks = await _context.Books.Take(4).ToListAsync();
                }

                // Genre Distribution (For Charts)
                var genreData = await _context.Books
                    .GroupBy(b => b.Genre)
                    .Select(g => new { Genre = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Monthly Borrowings (For Charts)
                var monthlyData = await _context.Loans
                    .GroupBy(l => new { Year = l.IssueDate.Year, Month = l.IssueDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Take(6)
                    .Select(g => new { 
                        MonthName = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}", 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                var viewModel = new DashboardViewModel
                {
                    TotalBooks = totalBooks,
                    TotalCopies = totalCopies,
                    AvailableCopies = availableCopies,
                    TotalMembers = totalMembers,
                    ActiveLoansCount = activeLoansCount,
                    OverdueLoansCount = overdueLoansCount,
                    TotalFinesCollected = totalFinesCollected,
                    TotalPendingFines = totalPendingFines,
                    PopularBooks = popularBooks,
                    OverdueLoans = overdueLoans.Take(5).ToList(),
                    RecentLoans = recentLoans,
                    GenreLabels = genreData.Select(g => g.Genre).ToList(),
                    GenreCounts = genreData.Select(g => g.Count).ToList(),
                    MonthlyLabels = monthlyData.Select(m => m.MonthName).ToList(),
                    MonthlyBorrowCounts = monthlyData.Select(m => m.Count).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred loading dashboard.");
                return View(new DashboardViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
