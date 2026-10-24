namespace LhlArt.Views.Shared;

// Wheel means scrolling the form, not editing the focused numeric/date/list field.
internal sealed class FormMouseWheelGuard : IMessageFilter,IDisposable
{
    readonly Form owner;readonly bool installed;
    readonly Dictionary<ScrollableControl,double> fractions=new();
    public FormMouseWheelGuard(Form owner,bool install=true){this.owner=owner;installed=install;if(install)Application.AddMessageFilter(this);}
    public bool PreFilterMessage(ref Message message)
    {
        if(message.Msg!=0x020A||owner.IsDisposed)return false;
        var target=Control.FromChildHandle(message.HWnd);
        if(target is null||!owner.Contains(target))return false;
        Control? editor=target;
        while(editor is not null&&editor!=owner&&editor is not(NumericUpDown or ComboBox or DateTimePicker))editor=editor.Parent;
        if(editor is null||editor==owner)return false;
        if(editor is ComboBox {DroppedDown:true})return false;
        var delta=unchecked((short)((message.WParam.ToInt64()>>16)&0xffff));
        ScrollParent(editor,delta);message.Result=IntPtr.Zero;return true;
    }
    void ScrollParent(Control editor,int delta)
    {
        for(var parent=editor.Parent;parent!=null;parent=parent.Parent){
            if(parent is not ScrollableControl {AutoScroll:true} scroll||!scroll.VerticalScroll.Visible)continue;
            var lines=SystemInformation.MouseWheelScrollLines;if(lines==0)return;
            var distance=delta/120d*(lines<0?scroll.ClientSize.Height:lines*Math.Max(16,scroll.Font.Height));
            var accumulated=fractions.GetValueOrDefault(scroll)+distance;var pixels=(int)accumulated;fractions[scroll]=accumulated-pixels;
            var maximum=Math.Max(0,scroll.VerticalScroll.Maximum-scroll.VerticalScroll.LargeChange+1);
            scroll.AutoScrollPosition=new Point(-scroll.AutoScrollPosition.X,Math.Clamp(-scroll.AutoScrollPosition.Y-pixels,0,maximum));
            return;
        }
    }
    public void Dispose(){if(installed)Application.RemoveMessageFilter(this);fractions.Clear();}
}
