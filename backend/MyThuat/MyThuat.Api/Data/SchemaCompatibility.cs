using Microsoft.EntityFrameworkCore;

namespace MyThuat.Api.Data;

internal static class SchemaCompatibility
{
    internal static async Task EnsureMemberBirthDateAsync(MyThuatDbContext db)
    {
        // A fresh database is created by --initialize. Leave it to that command
        // when the database does not exist yet or SQL Server cannot be reached.
        if(!await db.Database.CanConnectAsync())return;

        // Nullable preserves all existing accounts; no date is inferred or reset.
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ThanhVien', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.ThanhVien', N'NgaySinh') IS NULL
                ALTER TABLE dbo.ThanhVien ADD NgaySinh date NULL;
            """);
    }
}
