using System.Security.Claims;
using LibraryApi.Contracts;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BorrowingController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BorrowResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = db.BorrowRecords.AsNoTracking()
            .Include(record => record.Book)
            .Include(record => record.Member)
            .AsQueryable();
        if (!User.IsInRole(Roles.Admin))
        {
            var memberId = await GetCurrentMemberId(cancellationToken);
            if (memberId is null) return Forbid();
            query = query.Where(record => record.MemberId == memberId);
        }

        return Ok(await query.OrderByDescending(record => record.BorrowedAt)
            .Select(record => ToResponse(record))
            .ToListAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<BorrowResponse>> Issue(BorrowRequest request, CancellationToken cancellationToken)
    {
        if (request.DueAt.Kind == DateTimeKind.Unspecified || request.DueAt <= DateTime.UtcNow)
            return BadRequest(new { message = "Due date must be in the future and include a timezone." });

        int memberId;
        if (User.IsInRole(Roles.Admin))
        {
            if (request.MemberId is null or <= 0)
                return BadRequest(new { message = "MemberId is required for an administrator." });
            memberId = request.MemberId.Value;
        }
        else
        {
            var ownMemberId = await GetCurrentMemberId(cancellationToken);
            if (ownMemberId is null) return Forbid();
            memberId = ownMemberId.Value;
        }

        var book = await db.Books.AsNoTracking()
            .SingleOrDefaultAsync(book => book.Id == request.BookId, cancellationToken);
        if (book is null) return NotFound(new { message = "Book not found." });
        var member = await db.Members.SingleOrDefaultAsync(candidate => candidate.Id == memberId, cancellationToken);
        if (member is null) return NotFound(new { message = "Member not found." });

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var copiesUpdated = await db.Books
            .Where(candidate => candidate.Id == request.BookId && candidate.AvailableCopies > 0)
            .ExecuteUpdateAsync(update => update.SetProperty(
                candidate => candidate.AvailableCopies, candidate => candidate.AvailableCopies - 1),
                cancellationToken);
        if (copiesUpdated == 0)
            return Conflict(new { message = "No copies of this book are currently available." });

        var record = new BorrowRecord
        {
            BookId = book.Id,
            Book = await db.Books.SingleAsync(candidate => candidate.Id == book.Id, cancellationToken),
            MemberId = member.Id,
            Member = member,
            DueAt = request.DueAt.ToUniversalTime()
        };
        db.BorrowRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, ToResponse(record));
    }

    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        var record = await db.BorrowRecords
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (record is null) return NotFound();

        if (!User.IsInRole(Roles.Admin))
        {
            var memberId = await GetCurrentMemberId(cancellationToken);
            if (memberId is null || record.MemberId != memberId) return Forbid();
        }
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var returnedAt = DateTime.UtcNow;
        var updated = await db.BorrowRecords
            .Where(candidate => candidate.Id == id && candidate.ReturnedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(
                candidate => candidate.ReturnedAt, (DateTime?)returnedAt), cancellationToken);
        if (updated == 0)
            return Conflict(new { message = "This book has already been returned." });
        await db.Books
            .Where(book => book.Id == record.BookId)
            .ExecuteUpdateAsync(update => update.SetProperty(
                book => book.AvailableCopies, book => book.AvailableCopies + 1), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("overdue")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<OverdueBook>>> GetOverdue(CancellationToken cancellationToken)
    {
        var overdue = await db.OverdueBooks
            .FromSqlRaw("EXEC dbo.sp_GetOverdueBooks")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return Ok(overdue);
    }

    private async Task<int?> GetCurrentMemberId(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId)) return null;
        return await db.Members.Where(member => member.UserId == parsedUserId)
            .Select(member => (int?)member.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static BorrowResponse ToResponse(BorrowRecord record) =>
        new(record.Id, record.BookId, record.Book.Title, record.MemberId, record.Member.Name,
            record.BorrowedAt, record.DueAt, record.ReturnedAt);
}
