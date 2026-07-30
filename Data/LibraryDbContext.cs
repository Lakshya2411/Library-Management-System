using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using System;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for FineAmount
            modelBuilder.Entity<Loan>()
                .Property(l => l.FineAmount)
                .HasPrecision(18, 2);

            // Seed Data for Books
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", Genre = "Classic", TotalCopies = 5, AvailableCopies = 4, PublishYear = 1925, Publisher = "Scribner" },
                new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", Genre = "Fiction", TotalCopies = 4, AvailableCopies = 3, PublishYear = 1960, Publisher = "J. B. Lippincott & Co." },
                new Book { Id = 3, Title = "1984", Author = "George Orwell", ISBN = "9780451524935", Genre = "Dystopian", TotalCopies = 6, AvailableCopies = 5, PublishYear = 1949, Publisher = "Secker & Warburg" },
                new Book { Id = 4, Title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "9780547928227", Genre = "Fantasy", TotalCopies = 8, AvailableCopies = 6, PublishYear = 1937, Publisher = "George Allen & Unwin" },
                new Book { Id = 5, Title = "Harry Potter & the Sorcerer's Stone", Author = "J.K. Rowling", ISBN = "9780590353428", Genre = "Fantasy", TotalCopies = 10, AvailableCopies = 9, PublishYear = 1997, Publisher = "Scholastic" },
                new Book { Id = 6, Title = "A Brief History of Time", Author = "Stephen Hawking", ISBN = "9780553380163", Genre = "Science", TotalCopies = 3, AvailableCopies = 3, PublishYear = 1988, Publisher = "Bantam Books" },
                new Book { Id = 7, Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "9780316769174", Genre = "Classic", TotalCopies = 4, AvailableCopies = 2, PublishYear = 1951, Publisher = "Little, Brown" },
                new Book { Id = 8, Title = "Sapiens: A Brief History of Humankind", Author = "Yuval Noah Harari", ISBN = "9780062316097", Genre = "History", TotalCopies = 5, AvailableCopies = 4, PublishYear = 2011, Publisher = "Harper" }
            );

            // Seed Data for Members
            modelBuilder.Entity<Member>().HasData(
                new Member { Id = 1, FirstName = "Lakshya", LastName = "Agrawal", Email = "lakshya@example.com", PhoneNumber = "9876543210", JoinDate = new DateTime(2026, 1, 10), Status = "Active" },
                new Member { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "9887766554", JoinDate = new DateTime(2026, 2, 15), Status = "Active" },
                new Member { Id = 3, FirstName = "Robert", LastName = "Johnson", Email = "robert.j@example.com", PhoneNumber = "9776655443", JoinDate = new DateTime(2026, 3, 20), Status = "Suspended" },
                new Member { Id = 4, FirstName = "Emily", LastName = "Davis", Email = "emily.d@example.com", PhoneNumber = "9665544332", JoinDate = new DateTime(2026, 4, 1), Status = "Active" }
            );

            // Seed Data for Loans
            // We use fixed dates relative to current time to simulate returned, active and overdue items.
            // 2026-07-30 is our local current time.
            modelBuilder.Entity<Loan>().HasData(
                // 1. Returned loan (no fine)
                new Loan { Id = 1, BookId = 1, MemberId = 1, IssueDate = new DateTime(2026, 7, 1), DueDate = new DateTime(2026, 7, 15), ReturnDate = new DateTime(2026, 7, 12), FineAmount = 0.00m, IsReturned = true },
                // 2. Returned loan with fine
                new Loan { Id = 2, BookId = 2, MemberId = 2, IssueDate = new DateTime(2026, 7, 1), DueDate = new DateTime(2026, 7, 15), ReturnDate = new DateTime(2026, 7, 18), FineAmount = 3.00m, IsReturned = true },
                // 3. Active loan (within due date)
                new Loan { Id = 3, BookId = 3, MemberId = 4, IssueDate = new DateTime(2026, 7, 20), DueDate = new DateTime(2026, 8, 3), ReturnDate = null, FineAmount = 0.00m, IsReturned = false },
                // 4. Overdue loan (Issue 2026-07-10, Due 2026-07-24, Today 2026-07-30: 6 days overdue)
                new Loan { Id = 4, BookId = 4, MemberId = 1, IssueDate = new DateTime(2026, 7, 10), DueDate = new DateTime(2026, 7, 24), ReturnDate = null, FineAmount = 6.00m, IsReturned = false },
                // 5. Overdue loan (Issue 2026-07-05, Due 2026-07-19, Today 2026-07-30: 11 days overdue)
                new Loan { Id = 5, BookId = 7, MemberId = 2, IssueDate = new DateTime(2026, 7, 5), DueDate = new DateTime(2026, 7, 19), ReturnDate = null, FineAmount = 11.00m, IsReturned = false }
            );
        }
    }
}
