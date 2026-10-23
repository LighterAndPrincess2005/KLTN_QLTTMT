using LhlArt.Views.Shared;

namespace LhlArt.Views;
public sealed class LoginForm : Form
{
    readonly TextBox username=new(),password=new();
    readonly Label error=new();
    readonly Button submit;
    public event Action<string,string>? LoginRequested;
    public LoginForm()
    {
        Text="Đăng nhập | Mỹ Thuật Bụi";ClientSize=new Size(1080,680);MinimumSize=new Size(1000,700);
        StartPosition=FormStartPosition.CenterScreen;MaximizeBox=false;Font=Theme.Font();BackColor=Theme.Paper;
        AutoScaleMode=AutoScaleMode.Dpi;
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(22)};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,48));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,52));
        var brand=new StudioCard {Dock=DockStyle.Fill,Fill=Theme.Sage,Outline=false,Margin=new Padding(0,0,12,0),Padding=new Padding(30)};
        var brandLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,BackColor=Theme.Sage};
        foreach(float h in new[]{80f,112f,58f})brandLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,h));
        brandLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));brandLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,36));
        var identity=new FlowLayoutPanel {Dock=DockStyle.Fill,BackColor=Theme.Sage};
        var logo=Theme.Logo();logo.Size=new Size(66,66);logo.Margin=new Padding(0,0,12,0);identity.Controls.Add(logo);
        identity.Controls.Add(new Label {Text="Mỹ Thuật Bụi\nHọc vẽ cùng Bụi",AutoSize=true,Font=Theme.Font(12,FontStyle.Bold),ForeColor=Theme.Teal,Margin=new Padding(0,7,0,0)});
        brandLayout.Controls.Add(identity,0,0);
        brandLayout.Controls.Add(new Label {Text="Chạm vào màu sắc.\nĐánh thức chất riêng.",Dock=DockStyle.Fill,Font=Theme.Font(25,FontStyle.Bold),ForeColor=Theme.Navy},0,1);
        brandLayout.Controls.Add(new Label {Text="Một không gian để tự do sáng tạo.\nHọc vẽ cùng Bụi, mỗi ngày một chút.",Dock=DockStyle.Fill,Font=Theme.Font(10.5f),ForeColor=Theme.Muted},0,2);
        brandLayout.Controls.Add(new StudioPhoto {Dock=DockStyle.Fill,Margin=new Padding(0,12,0,6)},0,3);
        brandLayout.Controls.Add(new Label {Text="MỸ THUẬT  /  CẢM HỨNG  /  KẾT NỐI",Dock=DockStyle.Fill,Font=Theme.Font(8,FontStyle.Bold),ForeColor=Theme.Teal,TextAlign=ContentAlignment.BottomLeft},0,4);
        brand.Controls.Add(brandLayout);
        var right=new Panel {Dock=DockStyle.Fill,Padding=new Padding(40,32,20,10)};
        var fields=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,RowCount=13};fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        Label Copy(string text,float size,Color color,int height,bool bold=false)=>new(){Text=text,Dock=DockStyle.Fill,Height=height,AutoSize=false,Font=Theme.Font(size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=color,Margin=new Padding(0)};
        fields.Controls.Add(Copy("KHÔNG GIAN QUẢN LÝ",9,Theme.Teal,36,true));
        fields.Controls.Add(Copy("Chào mừng trở lại!",26,Theme.Ink,64,true));
        fields.Controls.Add(Copy("Quản lý lớp học và những hành trình sáng tạo.",10,Theme.Muted,56));
        fields.Controls.Add(Copy("Tên đăng nhập",10,Theme.Ink,26,true));
        username.PlaceholderText="Nhập tên đăng nhập";username.AccessibleName="Tên đăng nhập";username.TabIndex=0;
        fields.Controls.Add(new StudioInput(username,"Tài khoản") {Dock=DockStyle.Fill});
        fields.Controls.Add(Copy("Mật khẩu",10,Theme.Ink,26,true));
        password.UseSystemPasswordChar=true;password.PlaceholderText="Nhập mật khẩu";password.AccessibleName="Mật khẩu";password.TabIndex=1;
        fields.Controls.Add(new StudioInput(password,"Quản trị") {Dock=DockStyle.Fill});
        var reveal=new CheckBox {Text="Hiện mật khẩu",AutoSize=true,Height=32,ForeColor=Theme.Muted,Margin=new Padding(0,0,0,18)};
        reveal.CheckedChanged+=(_,_)=>password.UseSystemPasswordChar=!reveal.Checked;fields.Controls.Add(reveal);
        submit=Theme.Button("Đăng nhập   →",true);submit.AutoSize=false;submit.Height=52;submit.Dock=DockStyle.Fill;submit.Font=Theme.Font(12,FontStyle.Bold);submit.Margin=new Padding(0,0,0,8);
        fields.Controls.Add(submit);AcceptButton=submit;
        error.Height=60;error.Dock=DockStyle.Fill;error.ForeColor=Color.FromArgb(173,59,59);error.Font=Theme.Font(9);error.Margin=new Padding(0,5,0,0);fields.Controls.Add(error);
        fields.Controls.Add(Copy("Dành cho đội ngũ Mỹ Thuật Bụi.\nCần hỗ trợ? Liên hệ quản trị viên trung tâm.",9,Theme.Muted,50));
        right.Controls.Add(fields);layout.Controls.Add(brand,0,0);layout.Controls.Add(right,1,0);Controls.Add(layout);
        submit.Click+=(_,_)=>{error.Text="";if(string.IsNullOrWhiteSpace(username.Text)||password.Text.Length==0){error.Text="Bạn nhập tên đăng nhập và mật khẩu nhé.";return;}LoginRequested?.Invoke(username.Text.Trim(),password.Text);};
        Shown+=(_,_)=>username.Focus();
    }
    public void LoginFailed(string message){error.Text=message;password.SelectAll();password.Focus();}
    public void SetBusy(bool busy){UseWaitCursor=busy;foreach(Control control in Controls)control.Enabled=!busy;submit.Text=busy?"Đang đăng nhập…":"Đăng nhập   →";}
    public void LoginSucceeded(){DialogResult=DialogResult.OK;Close();}
}
