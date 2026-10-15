using LhlArt.Models;

namespace LhlArt.Services;
public partial class CenterService
{
    public IReadOnlyList<MonthlyFigures> MonthlyReport()
    {
        Need("Kế toán");
        var first = new DateTime(Now.Year, Now.Month, 1).AddMonths(-5);
        return Enumerable.Range(0, 6).Select(i =>
        {
            var month = first.AddMonths(i);
            var end = month.AddMonths(1);
            var payments = D.Payments.Where(x => x.At >= month && x.At < end && x.At <= Now).ToList();
            return new MonthlyFigures(month, payments.Where(x => x.Kind == "Thu").Sum(x => x.Amount), payments.Where(x => x.Kind == "Hoàn").Sum(x => x.Amount), D.MaterialSales.Where(x => x.At >= month && x.At < end && x.At <= Now).Sum(x => x.Total));
        }).ToList();
    }
}
