using LhlArt.Models;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views.Shared;

namespace LhlArt.Views;

internal sealed class CashReportForm : Form
{
    readonly DateTimePicker from = new() { Format=DateTimePickerFormat.Custom, CustomFormat="dd/MM/yyyy", Width=115 };
    readonly DateTimePicker to = new() { Format=DateTimePickerFormat.Custom, CustomFormat="dd/MM/yyyy", Width=115 };
    readonly ComboBox period = new() { DropDownStyle=ComboBoxStyle.DropDownList, Width=120 };
    readonly Button apply = Theme.Button("Áp dụng",true);
    readonly Label summary = new() { Dock=DockStyle.Top, Height=45, ForeColor=Theme.Navy, Padding=new Padding(12,10,0,0) };
    readonly Label status = new() { Dock=DockStyle.Bottom, Height=35, Padding=new Padding(12,5,0,0) };
    readonly Panel chart = new() { Dock=DockStyle.Fill };
    readonly DataGridView table = new() { Dock=DockStyle.Bottom, Height=180, ReadOnly=true, AllowUserToAddRows=false,
        RowHeadersVisible=false, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor=Color.White };
    readonly ArtApiClient api;
    sealed record PeriodRow(string Ky,string Thu,string Hoan,string Rong,string Cho);
    bool changingPreset;
    public CashReportForm(ArtApiClient api)
    {
        this.api=api;
        Text="Báo cáo thu / hoàn";
        Size=new Size(1060,730);
        MinimumSize=new Size(710,665);
        StartPosition=FormStartPosition.CenterParent;
        Font=Theme.Font();
        BackColor=Theme.Paper;
        var filters=new FlowLayoutPanel { Dock=DockStyle.Top, Height=84, Padding=new Padding(12,10,12,0), AutoScroll=true };
        void LabelFor(string text)=>filters.Controls.Add(new Label {Text=text,AutoSize=true,Margin=new Padding(6,7,4,0)});
        LabelFor("Kỳ báo cáo");filters.Controls.Add(period);
        LabelFor("Từ ngày");filters.Controls.Add(from);
        LabelFor("Đến ngày");filters.Controls.Add(to);filters.Controls.Add(apply);
        Controls.Add(chart);Controls.Add(table);Controls.Add(status);Controls.Add(summary);Controls.Add(filters);
        period.Items.AddRange(["Hôm nay","Tháng này","Năm nay","Tùy chọn"]);
        period.SelectedIndexChanged+=(_,_)=>SetPreset();
        from.ValueChanged+=(_,_)=>SelectCustom();
        to.ValueChanged+=(_,_)=>SelectCustom();
        apply.Click+=async(_,_)=>await LoadReport();
        period.SelectedIndex=1;
        Shown+=async(_,_)=>await LoadReport();
    }

    void SelectCustom(){if(!changingPreset)period.SelectedIndex=3;}
    void SetPreset()
    {
        if(period.SelectedIndex==3)return;
        var today=BusinessTime.Now.Date;
        changingPreset=true;
        from.Value=period.SelectedIndex switch {0=>today,1=>new DateTime(today.Year,today.Month,1),_=>new DateTime(today.Year,1,1)};
        to.Value=today;
        changingPreset=false;
    }

    async Task LoadReport()
    {
        var start=from.Value.Date;
        var end=to.Value.Date;
        if(end<start||(end-start).TotalDays>366)
        {
            status.Text="Chọn khoảng ngày hợp lệ, tối đa 367 ngày tính cả hai đầu.";
            return;
        }
        apply.Enabled=from.Enabled=to.Enabled=period.Enabled=false;
        status.Text="Đang tải báo cáo…";
        try
        {
            var daily=(end-start).TotalDays<14;
            var values=new List<MonthlyFigures>();
            var details=new List<PeriodRow>();
            for(var day=start;day<=end;)
            {
                var next=daily?day.AddDays(1):new DateTime(day.Year,day.Month,1).AddMonths(1);
                var last=next.AddDays(-1)<end?next.AddDays(-1):end;
                var result=await api.Read(FormattableString.Invariant($"api/bao-cao/thu-hoan?tuNgay={day:yyyy-MM-dd}&denNgay={last:yyyy-MM-dd}"));
                if(IsDisposed)return;
                decimal Amount(string name)=>ArtApiClient.Get(result,name).GetDecimal();
                var received=Amount("thuDaPhanBo");
                var refunded=Amount("hoanDaPhanBo");
                values.Add(new(day,received,refunded,0));
                details.Add(new(daily?day.ToString("dd/MM/yyyy"):$"{day:dd/MM/yyyy} – {last:dd/MM/yyyy}",
                    Theme.Money(received),Theme.Money(refunded),Theme.Money(received-refunded),
                    Theme.Money(Amount("thuChoDoiChieu"))));
                day=next;
            }
            foreach(Control child in chart.Controls.Cast<Control>().ToArray())child.Dispose();
            chart.Controls.Add(new StatisticsView(values,true,daily));
            table.DataSource=details;
            var headers=new[]{"Kỳ","Thu đã phân bổ","Hoàn đã phân bổ","Thu ròng","Thu chờ đối chiếu"};
            for(int i=0;i<table.Columns.Count;i++)table.Columns[i].HeaderText=headers[i];
            table.Columns[0].FillWeight=165;
            summary.Text=$"Thu ròng  {Theme.Money(values.Sum(x=>x.Tuition-x.Refunds))}   ·   {start:dd/MM/yyyy} đến {end:dd/MM/yyyy}";
            status.Text="Thu ròng = thu đã phân bổ trừ hoàn đã phân bổ. Tiền chờ đối chiếu hiển thị riêng.";
        }
        catch(Exception ex){if(!IsDisposed)status.Text="Chưa tải được kỳ đã chọn. "+ex.Message;}
        finally{if(!IsDisposed)apply.Enabled=from.Enabled=to.Enabled=period.Enabled=true;}
    }
}
