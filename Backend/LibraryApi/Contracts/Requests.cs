using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Contracts;

public sealed record RegisterRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [Phone, StringLength(30)] string? Phone);

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required] string Password);

public sealed record BookRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [Required, StringLength(160, MinimumLength = 1)] string Author,
    [StringLength(30)] string? Isbn,
    [StringLength(100)] string? Category,
    [Range(0, 100000)] int TotalCopies);

public sealed record MemberRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Phone, StringLength(30)] string? Phone);

public sealed record BorrowRequest(
    [Range(1, int.MaxValue)] int BookId,
    int? MemberId,
    [Required] DateTime DueAt);
