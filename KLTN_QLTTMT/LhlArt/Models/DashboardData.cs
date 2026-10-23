namespace LhlArt.Models;
public record Metric(string Name, string Value, string Detail);
public record DashboardLine(string Title, string Detail, string State);
public record DashboardShortcut(string Title, string Route);
public record DashboardProgress(string Title, int Value, int Total);
public record DashboardData(IReadOnlyList<Metric> Metrics, IReadOnlyList<DashboardLine> Highlights, IReadOnlyList<DashboardLine> Notices, string Greeting, string Period, string HighlightTitle, string HighlightRoute, IReadOnlyList<DashboardShortcut> Shortcuts, string ProgressTitle, IReadOnlyList<DashboardProgress> Progress);
