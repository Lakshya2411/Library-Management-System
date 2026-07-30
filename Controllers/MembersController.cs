using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly LibraryDbContext _context;

        public MembersController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var members = from m in _context.Members
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                members = members.Where(s => s.FirstName.Contains(searchString) || 
                                             s.LastName.Contains(searchString) || 
                                             s.Email.Contains(searchString) ||
                                             s.PhoneNumber.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                members = members.Where(x => x.Status == statusFilter);
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(await members.ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            // Fetch loan history for this member
            var loans = await _context.Loans
                .Where(l => l.MemberId == id)
                .Include(l => l.Book)
                .OrderByDescending(l => l.IssueDate)
                .ToListAsync();

            ViewBag.Loans = loans;

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber")] Member member)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                bool emailExists = await _context.Members.AnyAsync(m => m.Email == member.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                    return View(member);
                }

                member.JoinDate = DateTime.Today;
                member.Status = "Active";
                _context.Add(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member registered successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,JoinDate,Status")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists for a different user
                    bool emailExists = await _context.Members.AnyAsync(m => m.Email == member.Email && m.Id != member.Id);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Email address is already registered to another member.");
                        return View(member);
                    }

                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Member details updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
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
            return View(member);
        }

        // POST: Members/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            member.Status = (member.Status == "Active") ? "Suspended" : "Active";
            _context.Update(member);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Member status changed to {member.Status}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                // Check if member has active loans
                bool hasActiveLoans = await _context.Loans.AnyAsync(l => l.MemberId == id && !l.IsReturned);
                if (hasActiveLoans)
                {
                    TempData["Error"] = "Cannot delete member. They currently have active book loans.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member record deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
