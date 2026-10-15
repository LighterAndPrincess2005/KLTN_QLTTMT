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
    int row;
    public Action? ValidateAndSave { get; set; }

    public InputForm(string title)
    {
        Text = title + " | LHL ART";
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
        ok.Width = 136;
        var cancel = Theme.Button("Đóng");
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.Add(ok);
        footer.Controls.Add(cancel);
        CancelButton = cancel;
        AcceptButton = ok;
        ok.Click += (_, _) =>
        {
            try
            {
                ValidateAndSave?.Invoke();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(26, 16, 28, 20)
        };
        table.Dock = DockStyle.Top;
        table.AutoSize = true;
        table.ColumnCount = 2;
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(table);
        Controls.Add(scroll);
        Controls.Add(footer);
        Controls.Add(header);
    }

    public void Add(string key, string label, object? value = null, IEnumerable<Choice>? options = null, bool readOnly = false, bool password = false)
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
        else if (value is DateTime date)
            control = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = key is "Birth" or "MemberSince" ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm",
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
                UseSystemPasswordChar = password
            };
        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 12, 0, 12);
        control.Enabled = !readOnly;
        controls[key] = control;
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
    }

    public string TextValue(string key) => controls[key] is ComboBox combo ? (combo.SelectedItem as Choice)?.Id ?? "" : controls[key] is TextBox box && box.UseSystemPasswordChar ? box.Text : controls[key].Text.Trim();
    public decimal Number(string key) => ((NumericUpDown)controls[key]).Value;
    public bool Flag(string key) => ((CheckBox)controls[key]).Checked;
    public DateTime Date(string key) => ((DateTimePicker)controls[key]).Value;
    public void Fill<T>(T entity)
    {
        foreach (var property in typeof(T).GetProperties())
            if (controls.ContainsKey(property.Name))
                property.SetValue(entity, property.PropertyType == typeof(int) ? (object)(int)Number(property.Name) : property.PropertyType == typeof(decimal) ? Number(property.Name) : property.PropertyType == typeof(bool) ? Flag(property.Name) : property.PropertyType == typeof(DateTime) ? Date(property.Name) : TextValue(property.Name));
    }
}
