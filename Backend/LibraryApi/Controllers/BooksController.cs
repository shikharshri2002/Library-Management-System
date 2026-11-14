using LibraryApi.Contracts;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BooksController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = db.Books.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(book => book.Title.Contains(term) ||
                                        book.Author.Contains(term) ||
                                        (book.Isbn != null && book.Isbn.Contains(term)) ||
                                        (book.Category != null && book.Category.Contains(term)));
        }

        return Ok(await query.OrderBy(book => book.Title)
            .Select(book => ToResponse(book))
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await db.Books.AsNoTracking().SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
        return book is null ? NotFound() : Ok(ToResponse(book));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = Normalize(request.Isbn),
            Category = Normalize(request.Category),
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies
        };
        db.Books.Add(book);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            return Conflict(new { message = "A book with this ISBN already exists." });
        }

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, ToResponse(book));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, BookRequest request, CancellationToken cancellationToken)
    {
        var book = await db.Books.SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
        if (book is null) return NotFound();

        var borrowedCopies = book.TotalCopies - book.AvailableCopies;
        if (request.TotalCopies < borrowedCopies)
            return BadRequest(new { message = "Total copies cannot be less than the number currently borrowed." });

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Isbn = Normalize(request.Isbn);
        book.Category = Normalize(request.Category);
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.TotalCopies - borrowedCopies;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            return Conflict(new { message = "A book with this ISBN already exists." });
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await db.Books.SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
        if (book is null) return NotFound();
        if (await db.BorrowRecords.AnyAsync(record => record.BookId == id, cancellationToken))
            return Conflict(new { message = "Books with borrowing history cannot be deleted." });
        db.Books.Remove(book);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static BookResponse ToResponse(Book book) =>
        new(book.Id, book.Title, book.Author, book.Isbn, book.Category, book.TotalCopies, book.AvailableCopies);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool IsUniqueConstraint(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
