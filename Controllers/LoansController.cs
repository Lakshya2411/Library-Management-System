using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class LoansController : Controller
    {
        private readonly LibraryDbContext _context;

        public LoansController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var loans = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                loans = loans.Where(l => l.Book!.Title.Contains(searchString) || 
                                         l.Member!.FirstName.Contains(searchString) || 
                                         l.Member!.LastName.Contains(searchString));
            }

            var today = DateTime.Today;

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                {
                    loans = loans.Where(l => !l.IsReturned && today <= l.DueDate);
                }
                else if (statusFilter == "Overdue")
                {
                    loans = loans.Where(l => !l.IsReturned && today > l.DueDate);
                }
                else if (statusFilter == "Returned")
                {
                    loans = loans.Where(l => l.IsReturned);
                }
            }

            // Sync fines for overdue items dynamically prior to showing list
            var overdueActiveLoans = await _context.Loans
                .Where(l => !l.IsReturned && today > l.DueDate)
                .ToListAsync();

            if (overdueActiveLoans.Any())
            {
                foreach (var loan in overdueActiveLoans)
                {
                    decimal calcFine = (decimal)(today - loan.DueDate).Days * 1.00m;
                    if (loan.FineAmount != calcFine)
                    {
                        loan.FineAmount = calcFine;
                        _context.Update(loan);
                    }
                }
                await _context.SaveChangesAsync();
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(await loans.OrderByDescending(l => l.IssueDate).ToListAsync());
        }

        // GET: Loans/Create
        public async Task<IActionResult> Create()
        {
            var books = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Select(b => new { b.Id, Display = $"{b.Title} (by {b.Author}) [{b.AvailableCopies} left]" })
                .ToListAsync();

            var members = await _context.Members
                .Where(m => m.Status == "Active")
                .Select(m => new { m.Id, Display = $"{m.FirstName} {m.LastName} ({m.Email})" })
                .ToListAsync();

            ViewData["BookId"] = new SelectList(books, "Id", "Display");
            ViewData["MemberId"] = new SelectList(members, "Id", "Display");
            
            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,MemberId,DueDate")] Loan loan)
        {
            // Verify book has stock
            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null || book.AvailableCopies <= 0)
            {
                ModelState.AddModelError("BookId", "The selected book is currently unavailable.");
            }

            // Verify member is active
            var member = await _context.Members.FindAsync(loan.MemberId);
            if (member == null || member.Status != "Active")
            {
                ModelState.AddModelError("MemberId", "The selected member is not active or suspended.");
            }

            if (ModelState.IsValid)
            {
                loan.IssueDate = DateTime.Today;
                loan.IsReturned = false;
                loan.FineAmount = 0.00m;
                loan.ReturnDate = null;

                // Decrement stock
                if (book != null)
                {
                    book.AvailableCopies--;
                    _context.Update(book);
                }

                _context.Add(loan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Book checked out successfully!";
                return RedirectToAction(nameof(Index));
            }

            var books = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Select(b => new { b.Id, Display = $"{b.Title} (by {b.Author}) [{b.AvailableCopies} left]" })
                .ToListAsync();

            var members = await _context.Members
                .Where(m => m.Status == "Active")
                .Select(m => new { m.Id, Display = $"{m.FirstName} {m.LastName} ({m.Email})" })
                .ToListAsync();

            ViewData["BookId"] = new SelectList(books, "Id", "Display", loan.BookId);
            ViewData["MemberId"] = new SelectList(members, "Id", "Display", loan.MemberId);
            return View(loan);
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.IsReturned)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            loan.ReturnDate = today;
            loan.IsReturned = true;

            // Calculate overdue fine: $1.00 per day overdue
            if (today > loan.DueDate)
            {
                loan.FineAmount = (decimal)(today - loan.DueDate).Days * 1.00m;
            }
            else
            {
                loan.FineAmount = 0.00m;
            }

            // Increment book available count
            if (loan.Book != null)
            {
                loan.Book.AvailableCopies++;
                _context.Update(loan.Book);
            }

            _context.Update(loan);
            await _context.SaveChangesAsync();

            if (loan.FineAmount > 0)
            {
                TempData["Success"] = $"Book returned successfully! An overdue fine of ${loan.FineAmount:F2} was assessed.";
            }
            else
            {
                TempData["Success"] = "Book returned successfully with no fines!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Loans/PayFine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFine(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            loan.FineAmount = 0.00m;
            _context.Update(loan);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Fine paid and settled successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
