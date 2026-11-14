using LibraryApi.Contracts;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public sealed class MembersController(LibraryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> GetAll(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = db.Members.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(member => member.Name.Contains(term) || member.Email.Contains(term));
        }

        return Ok(await query.OrderBy(member => member.Name)
            .Select(member => new MemberResponse(member.Id, member.Name, member.Email, member.Phone, member.JoinedAt))
            .ToListAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(MemberRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Members.AnyAsync(member => member.Email == email, cancellationToken))
            return Conflict(new { message = "A member with this email already exists." });

        var member = new Member
        {
            Name = request.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim()
        };
        db.Members.Add(member);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            return Conflict(new { message = "A member with this email already exists." });
        }
        return CreatedAtAction(nameof(GetAll), new { }, ToResponse(member));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, MemberRequest request, CancellationToken cancellationToken)
    {
        var member = await db.Members.Include(member => member.User)
            .SingleOrDefaultAsync(member => member.Id == id, cancellationToken);
        if (member is null) return NotFound();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Members.AnyAsync(other => other.Id != id && other.Email == email, cancellationToken))
            return Conflict(new { message = "A member with this email already exists." });
        if (member.UserId is not null &&
            await db.Users.AnyAsync(user => user.Id != member.UserId && user.Email == email, cancellationToken))
            return Conflict(new { message = "An account with this email already exists." });

        member.Name = request.Name.Trim();
        member.Email = email;
        member.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        if (member.UserId is not null)
        {
            member.User!.Name = member.Name;
            member.User.Email = email;
        }
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            return Conflict(new { message = "A member or account with this email already exists." });
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var member = await db.Members.SingleOrDefaultAsync(member => member.Id == id, cancellationToken);
        if (member is null) return NotFound();
        if (await db.BorrowRecords.AnyAsync(record => record.MemberId == id, cancellationToken))
            return Conflict(new { message = "Members with borrowing history cannot be deleted." });
        db.Members.Remove(member);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static MemberResponse ToResponse(Member member) =>
        new(member.Id, member.Name, member.Email, member.Phone, member.JoinedAt);

    private static bool IsUniqueConstraint(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
