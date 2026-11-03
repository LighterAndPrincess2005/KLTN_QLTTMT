using System.Text.Json;
using LhlArt.Networking;
using LhlArt.Services;
using LhlArt.Views.Shared;

namespace LhlArt.Views;

internal sealed class ScheduleAvailabilityForm : Form
{
    sealed record Resource(string Id,string Name,string State,string Schedule,bool Available);

    public ScheduleAvailabilityForm(JsonElement data,DateTime start,DateTime end,
        List<JsonElement> rooms,List<JsonElement> teachers,long capacity,Action<string,string> select)
    {
        Text="Lịch phòng và giáo viên";
        Size=new Size(890,550);
        MinimumSize=new Size(660,390);
        StartPosition=FormStartPosition.CenterParent;
        Font=Theme.Font();
        BackColor=Theme.Paper;
        var sessions=ArtApiClient.Get(data,"buoiHoc").EnumerateArray().ToArray();
        var staff=ArtApiClient.Get(data,"giangDay").EnumerateArray().ToArray();
        var closed=ArtApiClient.Get(data,"ngayNghi").GetBoolean();
        string T(JsonElement x,string key)=>ArtApiClient.Text(x,key);
        bool B(JsonElement x)=>ArtApiClient.Get(x,"trungCa").GetBoolean();
        Resource Row(JsonElement x,bool isRoom)
        {
            var id=T(x,"id");
            var teacherSessions=isRoom?null:staff.Where(y=>T(y,"giaoVienId")==id).Select(y=>T(y,"buoiHocId")).ToHashSet();
            var bookings=sessions.Where(y=>isRoom?T(y,"phongHocId")==id:teacherSessions!.Contains(T(y,"id"))).ToArray();
            var tooSmall=isRoom&&ApiDataService.Id(x,"sucChua")<capacity;
            var free=!closed&&!tooSmall&&!bookings.Any(B);
            string Time(JsonElement item,string key)=>DateTime.Parse(T(item,key),System.Globalization.CultureInfo.InvariantCulture).ToString("HH:mm");
            var schedule=string.Join(Environment.NewLine,bookings.Select(y=>
                $"{Time(y,"batDau")}–{Time(y,"ketThuc")} · {T(y,"tenLop")}"+(B(y)?" (trùng ca)":"")));
            return new(id,T(x,isRoom?"tenPhong":"hoTen"),
                closed?"Ngày nghỉ":tooSmall?"Thiếu sức chứa":free?"Trống theo lịch":"Bận trong ca",
                schedule.Length==0?"Chưa có lịch trong ngày":schedule,free);
        }
        var tabs=new TabControl{Dock=DockStyle.Fill};
        void AddTab(string title,IEnumerable<Resource> resources,bool isRoom)
        {
            var rows=resources.OrderByDescending(x=>x.Available).ThenBy(x=>x.Name).ToArray();
            var page=new TabPage($"{title} ({rows.Count(x=>x.Available)} trống)");
            var grid=new DataGridView{Dock=DockStyle.Fill,AllowUserToAddRows=false,ReadOnly=true,RowHeadersVisible=false,
                AutoGenerateColumns=false,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells,
                AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.FullRowSelect,
                MultiSelect=false,BackgroundColor=Color.White};
            grid.DefaultCellStyle.WrapMode=DataGridViewTriState.True;
            foreach(var (key,label,weight) in new[]{("Name",title,33f),("State","Trạng thái ca",22f),("Schedule","Lịch trong ngày",45f)})
                grid.Columns.Add(new DataGridViewTextBoxColumn{DataPropertyName=key,HeaderText=label,FillWeight=weight,SortMode=DataGridViewColumnSortMode.NotSortable});
            grid.DataSource=rows;
            grid.RowPrePaint+=(_,e)=>{
                if(grid.Rows[e.RowIndex].DataBoundItem is Resource row)
                    grid.Rows[e.RowIndex].Cells[1].Style.ForeColor=row.Available?Theme.Teal:Color.Firebrick;
            };
            var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=54,Padding=new Padding(6),WrapContents=false};
            void Button(string caption,string key)
            {
                var button=Theme.Button(caption);
                button.Click+=(_,_)=>{
                    if(grid.CurrentRow?.DataBoundItem is not Resource {Available:true} row)
                    {
                        MessageBox.Show(this,"Hãy chọn dòng trống theo lịch.","Chọn lịch");
                        return;
                    }
                    select(key,row.Id);
                    Close();
                };
                actions.Controls.Add(button);
            }
            if(isRoom)Button("Dùng phòng này","room");
            else {Button("Chọn dạy chính","teacher");Button("Chọn trợ giảng","assistant");}
            page.Controls.Add(grid);
            page.Controls.Add(actions);
            tabs.TabPages.Add(page);
        }
        AddTab("Phòng học",rooms.Select(x=>Row(x,true)),true);
        AddTab("Giáo viên",teachers.Select(x=>Row(x,false)),false);
        var title=new Label{Dock=DockStyle.Top,Height=83,Padding=new Padding(12),ForeColor=Theme.Navy,
            Text=$"{start:dd/MM/yyyy} · {start:HH:mm}–{end:HH:mm}   |   Sĩ số tối đa: {capacity}\n"+
                 (closed?"Ngày này nằm trong lịch nghỉ của trung tâm.":"Trống theo lịch phân công của trung tâm. API sẽ kiểm tra lại tất cả buổi khi xác nhận.")};
        Controls.Add(tabs);Controls.Add(title);
    }
}
