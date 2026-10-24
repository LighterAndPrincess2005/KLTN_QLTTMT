namespace LhlArt.Views.Shared;

internal sealed class CollapsibleSidebarGroup : Panel
{
    readonly ActionButton header;readonly FlowLayoutPanel children=new();readonly string title;
    public IReadOnlyList<ActionButton> Buttons {get;}
    public bool Expanded {get;private set;}
    public event Action<CollapsibleSidebarGroup>? ToggleRequested;
    public CollapsibleSidebarGroup(string id,string title,List<ActionButton> buttons)
    {
        this.title=title;Buttons=buttons;Name="nav-group-"+id;Width=232;Margin=new Padding(0,3,0,3);BackColor=Color.White;
        header=new ActionButton{Name="nav-toggle-"+id,Text="▸  "+title,Navigation=true,Height=40,FlatStyle=FlatStyle.Flat,BackColor=Color.White,ForeColor=Theme.Ink,Font=Theme.Font(9.5f,FontStyle.Bold),AccessibleName=title,TabStop=true};header.FlatAppearance.BorderSize=0;
        children.Name="nav-children-"+id;children.FlowDirection=FlowDirection.TopDown;children.WrapContents=false;children.Padding=new Padding(10,3,0,3);children.BackColor=Color.FromArgb(248,249,245);children.Visible=false;
        foreach(var button in buttons){button.IconKey="";button.Height=38;button.Margin=new Padding(0,2,0,2);button.BackColor=children.BackColor;children.Controls.Add(button);}
        Controls.Add(children);Controls.Add(header);header.Click+=(_,_)=>ToggleRequested?.Invoke(this);
        SizeChanged+=(_,_)=>Fit();SetExpanded(false);
    }
    public bool ContainsRoute(string route)=>Buttons.Any(button=>button.Tag?.ToString()==route);
    public void SetExpanded(bool expanded)
    {
        Expanded=expanded;children.Visible=expanded;header.Text=(expanded?"▾  ":"▸  ")+title;
        header.AccessibleDescription=expanded?"Đang mở, bấm để thu gọn":"Đang thu gọn, bấm để mở danh sách";Fit();
    }
    public void SelectRoute(string route)
    {
        foreach(var button in Buttons){button.IsSelected=button.Tag?.ToString()==route;button.Invalidate();}
        header.ForeColor=ContainsRoute(route)?Theme.Teal:Theme.Ink;header.Invalidate();
    }
    void Fit()
    {
        header.Bounds=new Rectangle(0,0,ClientSize.Width,header.Height);children.Location=new Point(0,header.Height);
        children.Width=ClientSize.Width;var contentHeight=children.Padding.Vertical;
        foreach(var button in Buttons){button.Width=Math.Max(80,children.ClientSize.Width-children.Padding.Horizontal-button.Margin.Horizontal);contentHeight+=button.Height+button.Margin.Vertical;}
        children.Height=contentHeight;var wanted=header.Height+(Expanded?contentHeight:0);if(Height!=wanted)Height=wanted;
    }
}
