using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
    public DbSet<OverdueBook> OverdueBooks => Set<OverdueBook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<User>().Property(user => user.Email).IsRequired();
        modelBuilder.Entity<Book>().HasIndex(book => book.Isbn).IsUnique().HasFilter("[Isbn] IS NOT NULL");
        modelBuilder.Entity<Member>().HasIndex(member => member.Email).IsUnique();
        modelBuilder.Entity<Member>().HasIndex(member => member.UserId).IsUnique();
        modelBuilder.Entity<Member>().HasOne(member => member.User)
            .WithOne(user => user.Member).HasForeignKey<Member>(member => member.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BorrowRecord>().HasOne(record => record.Book)
            .WithMany(book => book.BorrowRecords).HasForeignKey(record => record.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BorrowRecord>().HasOne(record => record.Member)
            .WithMany(member => member.BorrowRecords).HasForeignKey(record => record.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BorrowRecord>().HasIndex(record => new { record.MemberId, record.ReturnedAt });
        modelBuilder.Entity<OverdueBook>().HasNoKey().ToView("OverdueBooks");
    }
}
