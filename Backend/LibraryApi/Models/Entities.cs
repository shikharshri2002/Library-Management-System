using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public sealed class User
{
    public int Id { get; set; }

    [MaxLength(120)]
    public required string Name { get; set; }

    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public required string PasswordHash { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = Roles.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Member? Member { get; set; }
}

public sealed class Book
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(160)]
    public required string Author { get; set; }

    [MaxLength(30)]
    public string? Isbn { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, 100000)]
    public int TotalCopies { get; set; }

    [Range(0, 100000)]
    public int AvailableCopies { get; set; }

    public ICollection<BorrowRecord> BorrowRecords { get; set; } = [];
}

public sealed class Member
{
    public int Id { get; set; }

    [MaxLength(120)]
    public required string Name { get; set; }

    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public ICollection<BorrowRecord> BorrowRecords { get; set; } = [];
}

public sealed class BorrowRecord
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
}

public sealed class OverdueBook
{
    public int BorrowRecordId { get; set; }
    public int BookId { get; set; }
    public required string Title { get; set; }
    public int MemberId { get; set; }
    public required string MemberName { get; set; }
    public required string MemberEmail { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public int DaysOverdue { get; set; }
}
