IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Isbn = N'978-0062315007')
BEGIN
    INSERT INTO dbo.Books (Title, Author, Isbn, Category, TotalCopies, AvailableCopies)
    VALUES (N'The Alchemist', N'Paulo Coelho', N'978-0062315007', N'Fiction', 4, 4);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Isbn = N'978-0140449136')
BEGIN
    INSERT INTO dbo.Books (Title, Author, Isbn, Category, TotalCopies, AvailableCopies)
    VALUES (N'Meditations', N'Marcus Aurelius', N'978-0140449136', N'Philosophy', 3, 3);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Books WHERE Isbn = N'978-0061120084')
BEGIN
    INSERT INTO dbo.Books (Title, Author, Isbn, Category, TotalCopies, AvailableCopies)
    VALUES (N'To Kill a Mockingbird', N'Harper Lee', N'978-0061120084', N'Classics', 5, 5);
END;
