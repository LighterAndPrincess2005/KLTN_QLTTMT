using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Checks;
// SQLite is only a disposable test store; the application uses SQL Server.
public sealed class CheckDbContext(DbContextOptions<MyThuatDbContext> options):MyThuatDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        if(Database.IsSqlServer())return;
        foreach(var type in b.Model.GetEntityTypes())
        {
            foreach(var check in type.GetCheckConstraints().ToList())type.RemoveCheckConstraint(check.Name!);
            foreach(var property in type.GetProperties())
            {
                var clr=Nullable.GetUnderlyingType(property.ClrType)??property.ClrType;
                property.SetColumnType(clr==typeof(string)||clr==typeof(DateTime)||clr==typeof(DateOnly)?"TEXT":
                    clr==typeof(byte[])?"BLOB":clr==typeof(decimal)?"TEXT":"INTEGER");
            }
            b.Entity(type.ClrType).Property("Id").HasColumnType("INTEGER");
            b.Entity(type.ClrType).Property("RowVersion").ValueGeneratedNever();
        }
    }
    public override Task<int> SaveChangesAsync(CancellationToken ct=default)
    {
        if(Database.IsSqlServer())return base.SaveChangesAsync(ct);
        foreach(var entry in ChangeTracker.Entries<EntityBase>())
            if(entry.State is EntityState.Added or EntityState.Modified)entry.Entity.RowVersion=System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
        return base.SaveChangesAsync(ct);
    }
}
