using LhlArt.Views.Shared;

namespace LhlArt.Views;
public record Choice(string Id, string Label)
{
    public override string ToString() => Label;
}

public class InputForm : Form
{
    readonly TableLayoutPanel table = new();
    readonly Dictionary<string, Control> controls = new();
    readonly Dictionary<string, Label> errors = new();
    readonly Label formError = new() { AutoSize=true,ForeColor=Color.Firebrick,Visible=false,UseMnemonic=false,Margin=new Padding(0,0,0,12) };
    readonly Button saveButton;
    FormMouseWheelGuard? wheelGuard;
    public bool ShowSaveButton { set=>saveButton.Visible=value; }
    int row;
    public Action? ValidateAndSave { get; set; }
    public Func<Task>? SaveAsync { get; set; }
    public Func<IReadOnlyDictionary<string,string>>? FieldValidation { get; set; }
    public Func<Exception,IReadOnlyDictionary<string,string>>? ServerErrorFields { get; set; }
    bool saving;
    public void SetSaveStatus(string message)
    {
        formError.ForeColor=Theme.Teal;formError.Text=message;formError.Visible=true;
        if(table.Parent is ScrollableControl scroll)scroll.ScrollControlIntoView(formError);
    }

    public InputForm(string title)
    {
        Text = title + " | Mỹ Thuật Bụi";
        Font = Theme.Font();
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 720);
        MinimumSize = new Size(650, 500);
        BackColor = Color.White;
        AutoScaleMode = AutoScaleMode.Dpi;
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 94,
            Padding = new Padding(24, 16, 24, 12),
            BackColor = Theme.Paper
        };
        var logo = Theme.Logo();
        logo.Dock = DockStyle.Right;
        logo.Width = 60;
        header.Controls.Add(new Label { UseMnemonic = false, Text = title, Dock = DockStyle.Fill, Font = Theme.Font(17, FontStyle.Bold), ForeColor = Theme.Navy, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true });
        header.Controls.Add(logo);
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 74,
            Padding = new Padding(18, 9, 18, 10),
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Theme.Paper
        };
        var ok = Theme.Button("Xác nhận", true);
        ok.Name="confirm-save";
        ok.Width = 136;
        saveButton=Theme.Button("Lưu");saveButton.Name="save-continue";saveButton.Width=110;saveButton.Visible=false;
        var cancel = Theme.Button("Đóng");
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.Add(ok);
        footer.Controls.Add(saveButton);
        footer.Controls.Add(cancel);
        CancelButton = cancel;
        AcceptButton = ok;
        FormClosing += (_, e) => { if (saving) e.Cancel = true; };
        async Task Submit(bool closeAfterSave)
        {
            if(saving)return;
            try
            {
                if(FieldValidation is not null&&!ValidateFields(true))return;
                ValidateAndSave?.Invoke();
                saving = true; ok.Enabled = false; saveButton.Enabled=false; cancel.Enabled = false; table.Enabled = false;
                ok.Text="Đang lưu…";SetSaveStatus("Đang gửi dữ liệu tới API. Vui lòng chờ…");
                if (SaveAsync is not null) await SaveAsync();
                saving = false;
                if(closeAfterSave){DialogResult = DialogResult.OK;Close();}
                else {formError.ForeColor=Theme.Teal;formError.Text="Đã lưu. Bạn có thể tiếp tục chỉnh sửa hoặc bấm Xác nhận để đóng.";formError.Visible=true;}
            }
            catch (Exception ex)
            {
                if(FieldValidation is not null){
                    ShowFieldErrors(ServerErrorFields?.Invoke(ex)??new Dictionary<string,string>());
                    formError.ForeColor=Color.Firebrick;formError.Text=ex.Message;formError.Visible=true;
                }
                else MessageBox.Show(this, ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { saving = false; if (!IsDisposed) { ok.Text="Xác nhận";ok.Enabled = true;saveButton.Enabled=true; cancel.Enabled = true; table.Enabled = true; } }
        }
        ok.Click+=async(_,_)=>await Submit(true);saveButton.Click+=async(_,_)=>await Submit(false);
        var scroll = new Panel
        {
            Name="form-scroll",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(26, 16, 28, 20)
        };
        table.Dock = DockStyle.Top;
        table.AutoSize = true;
        table.ColumnCount = 2;
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));table.Controls.Add(formError,0,row++);table.SetColumnSpan(formError,2);
        table.SizeChanged+=(_,_)=>{
            var width=Math.Max(120,table.ClientSize.Width-245);
            formError.MaximumSize=new Size(Math.Max(120,table.ClientSize.Width-10),0);
            foreach(var error in errors.Values)error.MaximumSize=new Size(width,0);
        };
        scroll.Controls.Add(table);
        Controls.Add(scroll);
        Controls.Add(footer);
        Controls.Add(header);
        wheelGuard=new FormMouseWheelGuard(this);
    }

    public void Add(string key, string label, object? value = null, IEnumerable<Choice>? options = null, bool readOnly = false, bool password = false, bool multiline = false)
    {
        Control control;
        if (options != null)
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DropDownWidth = 440,
                IntegralHeight = false,
                MaxDropDownItems = 12
            };
            foreach (var option in options)
                combo.Items.Add(option);
            int selected = combo.Items.Cast<Choice>().ToList().FindIndex(x => x.Id == Convert.ToString(value));
            combo.SelectedIndex = selected >= 0 ? selected : combo.Items.Count > 0 ? 0 : -1;
            control = combo;
        }
        else if (value is bool flag)
            control = new CheckBox
            {
                Checked = flag,
                Text = "Có / đã xác nhận",
                AutoSize = true
            };
        else if(value is TimeSpan time)
            control=new DateTimePicker {Format=DateTimePickerFormat.Custom,CustomFormat="HH:mm",ShowUpDown=true,Value=DateTime.Today.Add(time)};
        else if (value is DateTime date)
            control = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = key is "Birth" or "MemberSince" or "NgaySinh" or "HieuLucTu" or "NgayKhaiGiangDuKien" ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm",
                Value = date.Year < 1900 ? DateTime.Today : date
            };
        else if (value is int || value is decimal)
        {
            bool money = Theme.IsMoney(key);
            control = new NumericUpDown
            {
                Minimum = -1000000000,
                Maximum = 1000000000,
                DecimalPlaces = value is decimal && !money ? 2 : 0,
                ThousandsSeparator = true,
                Value = Convert.ToDecimal(value),
                TextAlign = HorizontalAlignment.Right
            };
        }
        else
            control = new TextBox
            {
                Text = Convert.ToString(value) ?? "",
                UseSystemPasswordChar = password,
                Multiline = multiline,
                Height = multiline ? 85 : 28,
                ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
                ImeMode = password ? ImeMode.Disable : ImeMode.On,
                ShortcutsEnabled = true
            };
        control.Dock = DockStyle.Top;
        control.Name=key;
        control.Margin = new Padding(0, 12, 0, 12);
        control.Enabled = !readOnly;
        controls[key] = control;
        if (key is "NgaySinh" or "Birth" && control is DateTimePicker birth) { birth.MinDate = new DateTime(1900, 1, 1); birth.MaxDate = Services.BusinessTime.Now.Date; }
        Control field = control;
        if ((value is int || value is decimal) && Theme.IsMoney(key))
        {
            var moneyField = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Margin = new Padding(0, 12, 0, 12)
            };
            moneyField.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            moneyField.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            control.Margin = Padding.Empty;
            moneyField.Controls.Add(control, 0, 0);
            moneyField.Controls.Add(new Label { Text = "₫", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Muted, Margin = Padding.Empty }, 1, 0);
            field = moneyField;
        }

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label { UseMnemonic = false, Text = label, AutoSize = true, MaximumSize = new Size(215, 0), ForeColor = Theme.Ink, Margin = new Padding(0, 15, 18, 12) }, 0, row);
        table.Controls.Add(field, 1, row++);
        var errorLabel=new Label {AutoSize=true,ForeColor=Color.Firebrick,Visible=false,UseMnemonic=false,AccessibleName="Lỗi: "+label,Margin=new Padding(0,0,0,10),MaximumSize=new Size(400,0)};
        errors[key]=errorLabel;table.RowStyles.Add(new RowStyle(SizeType.AutoSize));table.Controls.Add(errorLabel,1,row++);
    }

    public void Changed(string key, Action action)
    {
        if (controls[key] is ComboBox combo) combo.SelectedIndexChanged += (_, _) => action();
        else if (controls[key] is CheckBox check) check.CheckedChanged += (_, _) => action();
        else if(controls[key] is DateTimePicker date)date.ValueChanged+=(_,_)=>action();
        else if(controls[key] is NumericUpDown number)number.ValueChanged+=(_,_)=>action();
        else controls[key].TextChanged += (_, _) => action();
    }
    public void Set(string key, object? value)
    {
        var control = controls[key];
        if (control is ComboBox combo) combo.SelectedIndex = combo.Items.Cast<Choice>().ToList().FindIndex(x => x.Id == Convert.ToString(value));
        else if (control is DateTimePicker date && value is DateTime time) date.Value = time;
        else if(control is DateTimePicker clock&&value is TimeSpan clockTime)clock.Value=DateTime.Today.Add(clockTime);
        else if (control is NumericUpDown number) number.Value = Convert.ToDecimal(value);
        else if (control is CheckBox check) check.Checked = Convert.ToBoolean(value);
        else control.Text = Convert.ToString(value) ?? "";
    }
    public void Options(string key, IEnumerable<Choice> options)
    {
        var combo = (ComboBox)controls[key];
        combo.Items.Clear(); foreach (var option in options) combo.Items.Add(option);
        combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
    }
    public void Enable(string key, bool enabled) => controls[key].Enabled = enabled;

    public string TextValue(string key) => controls[key] is ComboBox combo ? (combo.SelectedItem as Choice)?.Id ?? "" : controls[key] is TextBox box && box.UseSystemPasswordChar ? box.Text : controls[key].Text.Trim();
    public decimal Number(string key) => ((NumericUpDown)controls[key]).Value;
    public bool Flag(string key) => ((CheckBox)controls[key]).Checked;
    public DateTime Date(string key) => ((DateTimePicker)controls[key]).Value;
    public TimeSpan Time(string key)=>((DateTimePicker)controls[key]).Value.TimeOfDay;
    public string FieldError(string key)=>errors[key].Text;
    public bool IsFieldEnabled(string key)=>controls[key].Enabled;
    public bool ValidateFields(bool focusFirst=false)
    {
        var failures=FieldValidation?.Invoke()??new Dictionary<string,string>();ShowFieldErrors(failures);
        formError.ForeColor=Color.Firebrick;
        formError.Text=failures.Count>0?"Vui lòng sửa các mục có cảnh báo màu đỏ bên dưới.":"";formError.Visible=failures.Count>0;
        if(focusFirst&&failures.Keys.FirstOrDefault(controls.ContainsKey) is string key){controls[key].Focus();if(table.Parent is ScrollableControl scroll)scroll.ScrollControlIntoView(controls[key]);}
        return failures.Count==0;
    }
    public void ShowFieldErrors(IReadOnlyDictionary<string,string> failures)
    {
        table.SuspendLayout();foreach(var pair in errors){pair.Value.Text=failures.GetValueOrDefault(pair.Key,"");pair.Value.Visible=pair.Value.Text.Length>0;}table.ResumeLayout(true);
    }
    public void Fill<T>(T entity)
    {
        foreach (var property in typeof(T).GetProperties())
            if (controls.ContainsKey(property.Name))
                property.SetValue(entity, property.PropertyType == typeof(int) ? (object)(int)Number(property.Name) : property.PropertyType == typeof(decimal) ? Number(property.Name) : property.PropertyType == typeof(bool) ? Flag(property.Name) : property.PropertyType == typeof(DateTime) ? Date(property.Name) : TextValue(property.Name));
    }
    protected override void Dispose(bool disposing){if(disposing){wheelGuard?.Dispose();wheelGuard=null;}base.Dispose(disposing);}
}
