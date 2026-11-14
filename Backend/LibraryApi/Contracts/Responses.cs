namespace LibraryApi.Contracts;

public sealed record AuthResponse(string Token, DateTime ExpiresAt, string Role, string Name);

public sealed record BookResponse(
    int Id, string Title, string Author, string? Isbn, string? Category, int TotalCopies, int AvailableCopies);

public sealed record MemberResponse(
    int Id, string Name, string Email, string? Phone, DateTime JoinedAt);

public sealed record BorrowResponse(
    int Id, int BookId, string BookTitle, int MemberId, string MemberName,
    DateTime BorrowedAt, DateTime DueAt, DateTime? ReturnedAt);
