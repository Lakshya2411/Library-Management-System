using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString, string genreFilter)
        {
            var books = from b in _context.Books
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString) || s.Author.Contains(searchString) || s.ISBN.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(genreFilter))
            {
                books = books.Where(x => x.Genre == genreFilter);
            }

            var genres = await _context.Books
                .Select(b => b.Genre)
                .Distinct()
                .ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentGenre = genreFilter;

            return View(await books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            // Fetch loan history for this book
            var loans = await _context.Loans
                .Where(l => l.BookId == id)
                .Include(l => l.Member)
                .OrderByDescending(l => l.IssueDate)
                .ToListAsync();

            ViewBag.Loans = loans;

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Genre,TotalCopies,PublishYear,Publisher")] Book book)
        {
            if (ModelState.IsValid)
            {
                // For a new book, available copies matches total copies initially
                book.AvailableCopies = book.TotalCopies;
                _context.Add(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Book added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Genre,TotalCopies,AvailableCopies,PublishYear,Publisher")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Adjust AvailableCopies if TotalCopies changed
                    var originalBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    if (originalBook != null)
                    {
                        int diff = book.TotalCopies - originalBook.TotalCopies;
                        book.AvailableCopies = originalBook.AvailableCopies + diff;
                        if (book.AvailableCopies < 0)
                        {
                            book.AvailableCopies = 0; // fallback safety
                        }
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Book details updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // Check if book has active loans
                bool hasActiveLoans = await _context.Loans.AnyAsync(l => l.BookId == id && !l.IsReturned);
                if (hasActiveLoans)
                {
                    TempData["Error"] = "Cannot delete book. It is currently borrowed by a member.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Book deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
