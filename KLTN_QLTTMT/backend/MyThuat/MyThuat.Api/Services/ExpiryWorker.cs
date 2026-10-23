using Microsoft.EntityFrameworkCore;
namespace MyThuat.Api.Services;
public sealed class ExpiryWorker(IServiceScopeFactory scopes,ILogger<ExpiryWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));
        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope=scopes.CreateScope();
                var db=scope.ServiceProvider.GetRequiredService<Data.MyThuatDbContext>();
                await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,stoppingToken);
                await scope.ServiceProvider.GetRequiredService<EnrollmentService>().Expire(stoppingToken);
                await tx.CommitAsync(stoppingToken);
            }
            catch(OperationCanceledException)when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception ex){logger.LogError(ex,"Không giải phóng được chỗ/ưu đãi hết hạn; sẽ thử lại ở lượt sau.");}
        }
    }
}
