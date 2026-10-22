using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
namespace MyThuat.Api.Services;

public static class MainDatabaseSetup
{
    public static async Task<string> Backup(MyThuatDbContext db,string root)
    {
        var connection=(SqlConnection)db.Database.GetDbConnection();
        await connection.OpenAsync();var name=connection.Database;
        if(string.IsNullOrWhiteSpace(name)||name is "master" or "model" or "msdb" or "tempdb")throw new InvalidOperationException("Không sao lưu database hệ thống.");
        var directory=Path.GetFullPath(Path.Combine(root,".local","backups"));Directory.CreateDirectory(directory);
        var path=Path.Combine(directory,$"{name}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");
        await using(var cmd=connection.CreateCommand()){
            cmd.CommandTimeout=180;cmd.CommandText=$"BACKUP DATABASE [{name.Replace("]","]]")}] TO DISK=@path WITH COPY_ONLY, CHECKSUM, INIT";cmd.Parameters.AddWithValue("@path",path);await cmd.ExecuteNonQueryAsync();
        }
        await using(var cmd=connection.CreateCommand()){
            cmd.CommandTimeout=180;cmd.CommandText="RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM";cmd.Parameters.AddWithValue("@path",path);await cmd.ExecuteNonQueryAsync();
        }
        await connection.CloseAsync();Console.WriteLine("Sao lưu đã xác minh: "+path);return path;
    }
}
