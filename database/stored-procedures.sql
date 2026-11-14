CREATE OR ALTER PROCEDURE dbo.sp_GetOverdueBooks
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        br.Id AS BorrowRecordId,
        b.Id AS BookId,
        b.Title,
        m.Id AS MemberId,
        m.Name AS MemberName,
        m.Email AS MemberEmail,
        br.BorrowedAt,
        br.DueAt,
        DATEDIFF(DAY, br.DueAt, SYSUTCDATETIME()) AS DaysOverdue
    FROM dbo.BorrowRecords AS br
    INNER JOIN dbo.Books AS b ON b.Id = br.BookId
    INNER JOIN dbo.Members AS m ON m.Id = br.MemberId
    WHERE br.ReturnedAt IS NULL
      AND br.DueAt < SYSUTCDATETIME()
    ORDER BY br.DueAt ASC;
END;
