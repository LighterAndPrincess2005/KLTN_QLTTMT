using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Views.Shared;

namespace LhlArt.Views;

internal sealed class StudioDashboard : TableLayoutPanel
{
    readonly ArtApiClient api;
    readonly Label connection=new();
    readonly List<Label> values=new();
    readonly FlowLayoutPanel links=new();
    public StudioDashboard(ArtApiClient client,IReadOnlyList<(string Name,string Route,bool Paged)> destinations,Func<string,string,bool,Task> open)
    {
        api=client;AutoSize=true;ColumnCount=1;RowCount=6;BackColor=Theme.Paper;Margin=new Padding(0);
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(float h in new[]{245f,52f,132f,65f,190f,48f})RowStyles.Add(new RowStyle(SizeType.Absolute,h));
        var hero=new StudioCard {Dock=DockStyle.Fill,Fill=Theme.Navy,Outline=false,Padding=new Padding(30,22,22,22),Margin=new Padding(0)};
        var art=new StudioPhoto {Dock=DockStyle.Right,Width=270};hero.Controls.Add(art);
        var copy=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,BackColor=Theme.Navy};
        foreach(float h in new[]{28f,55f,52f,50f})copy.RowStyles.Add(new RowStyle(SizeType.Absolute,h));
        copy.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        copy.Controls.Add(TextLabel("HỌC VẼ CÙNG BỤI",9,Theme.Gold,true),0,0);
        copy.Controls.Add(TextLabel("Một ngày mới tại Bụi.",25,Color.White,true),0,1);
        copy.Controls.Add(TextLabel("Cùng tạo nên một ngày thật nhiều cảm hứng\nvà chăm chút từng hành trình học tập.",11,Color.FromArgb(221,232,218)),0,2);
        var date=new Label {Text=Services.BusinessTime.Now.ToString("dddd, dd 'tháng' MM, yyyy",Theme.Culture),AutoSize=true,BackColor=Theme.Navy,ForeColor=Theme.Gold,Font=Theme.Font(9,FontStyle.Bold),Padding=new Padding(0,13,0,0)};copy.Controls.Add(date,0,3);
        hero.Controls.Add(copy);Controls.Add(hero,0,0);
        Controls.Add(TextLabel("Nhịp sống trung tâm",15,Theme.Ink,true,16),0,1);
        var stats=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,Margin=new Padding(0)};
        var names=api.Has(32)?new[]{"Học viên hoạt động","Giáo viên","Lớp tuyển sinh","Lớp đang học"}:api.Has(8)?new[]{"Buổi được phân công","Buổi học hôm nay","Buổi sắp tới","Buổi đã tổ chức"}:new[]{"Vai trò","Không gian","Dữ liệu","Phiên làm việc"};
        for(int i=0;i<4;i++)
        {
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
            var card=new StudioCard {Dock=DockStyle.Fill,Padding=new Padding(20,16,15,10),Margin=new Padding(i==0?0:7,0,i==3?0:7,0)};
            var number=new Label {Text="—",Dock=DockStyle.Top,Height=52,Font=Theme.Font(28,FontStyle.Bold),ForeColor=i==2?Theme.Coral:Theme.Teal};values.Add(number);
            var name=new Label {Text=names[i],Dock=DockStyle.Bottom,Height=28,Font=Theme.Font(9.5f),ForeColor=Theme.Muted};
            card.Controls.Add(number);card.Controls.Add(name);stats.Controls.Add(card,i,0);
        }
        Controls.Add(stats,0,2);
        Controls.Add(TextLabel("Bạn muốn bắt đầu từ đâu?",16,Theme.Ink,true,24),0,3);
        links.Dock=DockStyle.Fill;links.WrapContents=false;links.Margin=new Padding(0);
        var priority=new[]{"api/quan-ly/hoc-vien","api/quan-ly/lop-hoc","api/lich-hoc"};
        var choices=destinations.Where(x=>x.Route!="api/bao-cao/tong-quan"&&x.Route!="api/dang-ky/quan-ly").OrderBy(x=>Array.IndexOf(priority,x.Route) is var rank&&rank>=0?rank:100).Take(3).ToArray();
        foreach(var item in choices)
        {
            var card=new StudioCard {Width=260,Height=174,Padding=new Padding(18),Margin=new Padding(0,0,14,0)};
            var caption=new Label {Text=item.Name,UseMnemonic=false,Height=32,Dock=DockStyle.Top,Font=Theme.Font(12,FontStyle.Bold),ForeColor=Theme.Ink};
            string hint=item.Name.Contains("Lịch")?"Theo dõi buổi học và điểm danh.":item.Name.Contains("viên")?"Tra cứu và chăm sóc hồ sơ.":item.Name.Contains("phản",StringComparison.OrdinalIgnoreCase)?"Kết nối, lắng nghe và hỗ trợ.":"Tra cứu thông tin của trung tâm.";
            var description=new Label {Text=hint,Height=43,Dock=DockStyle.Top,Font=Theme.Font(9),ForeColor=Theme.Muted};
            var button=Theme.Button("Mở danh sách   →");button.Dock=DockStyle.Bottom;button.Height=40;button.Click+=async(_,_)=>await open(item.Name,item.Route,item.Paged);
            card.Controls.Add(description);card.Controls.Add(caption);card.Controls.Add(button);links.Controls.Add(card);
        }
        links.SizeChanged+=(_,_)=>{foreach(Control card in links.Controls)card.Width=Math.Max(210,(links.ClientSize.Width-28)/Math.Max(1,choices.Length)-10);};
        Controls.Add(links,0,4);
        connection.Text="Đang tải thông tin trung tâm…";connection.Dock=DockStyle.Fill;connection.Font=Theme.Font(9);connection.ForeColor=Theme.Muted;connection.TextAlign=ContentAlignment.MiddleLeft;Controls.Add(connection,0,5);
    }
    static Label TextLabel(string text,float size,Color color,bool bold=false,int top=0)=>new(){Text=text,Dock=DockStyle.Fill,Font=Theme.Font(size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=color,Padding=new Padding(0,top,0,0),Margin=new Padding(0)};
    public async Task LoadOverview()
    {
        try
        {
            if(api.Has(32))
            {
                var overview=await api.Read("api/bao-cao/tong-quan");
                var keys=new[]{"hocVien","giaoVien","lopTuyenSinh","lopDangHoc"};
                for(int i=0;i<keys.Length;i++)values[i].Text=ArtApiClient.Text(overview,keys[i]);
            }
            else if(api.Has(8))
            {
                var data=await api.Read("api/lich-hoc");var sessions=data.EnumerateArray().ToArray();var now=Services.BusinessTime.Now;
                DateTime Start(JsonElement row)=>ArtApiClient.Get(row,"batDau").GetDateTime();
                var counts=new[]{sessions.Length,sessions.Count(x=>Start(x).Date==now.Date),sessions.Count(x=>Start(x)>now),sessions.Count(x=>ArtApiClient.Text(x,"trangThai")=="DA_TO_CHUC")};
                for(int i=0;i<counts.Length;i++)values[i].Text=counts[i].ToString();
            }
            else
            {
                _=await api.Read("api/tai-khoan/toi");
                var texts=new[]{api.Role=="THU_NGAN"?"Thu ngân":"Kho","Mỹ Thuật Bụi","Trực tiếp","Đã kết nối"};
                for(int i=0;i<texts.Length;i++){values[i].Text=texts[i];values[i].Font=Theme.Font(15,FontStyle.Bold);}
            }
            connection.Text="●  Đã cập nhật từ trung tâm   ·   "+Services.BusinessTime.Now.ToString("HH:mm, dd/MM/yyyy");connection.ForeColor=Theme.Teal;
        }
        catch(ApiSessionExpiredException){throw;}
        catch(Exception ex){connection.Text="Chưa tải được thông tin. Về Trang chủ để thử lại. "+ex.Message;connection.ForeColor=Color.FromArgb(173,59,59);}
    }
}
