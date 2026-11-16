using System.Globalization;
using System.ComponentModel;

namespace Stroik;

// One native editable-text accessibility element, without a spin-button parent/duplicate value.
internal sealed class EditableNumber : TextBox
{
    private decimal _minimum, _maximum = 100, _value, _increment = 1;
    private int _decimalPlaces;
    private bool _formatting;
    internal event EventHandler? ValueChanged;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal decimal Minimum { get => _minimum; set { _minimum = value; if (_maximum < value) _maximum = value; Value = _value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal decimal Maximum { get => _maximum; set { _maximum = value; if (_minimum > value) _minimum = value; Value = _value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal decimal Increment { get => _increment; set => _increment = value; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal int DecimalPlaces { get => _decimalPlaces; set { _decimalPlaces = value; FormatValue(); } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal decimal Value { get => _value; set { Assign(value); } }
    internal EditableNumber() { AccessibleRole = AccessibleRole.Text; Text = "0"; }
    private void Assign(decimal value)
    {
        decimal next = Math.Clamp(decimal.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero), Minimum, Maximum);
        bool changed = next != _value; _value = next; FormatValue();
        if (changed) ValueChanged?.Invoke(this, EventArgs.Empty);
    }
    private void FormatValue() { _formatting = true; Text = _value.ToString("0" + (DecimalPlaces > 0 ? "." + new string('#', DecimalPlaces) : ""), CultureInfo.CurrentCulture); _formatting = false; }
    private void Commit()
    {
        if (_formatting) return;
        if (decimal.TryParse(Text.Trim().Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal number)) Assign(number);
        else FormatValue();
    }
    protected override void OnLeave(EventArgs e) { Commit(); base.OnLeave(e); }
    protected override void OnValidating(CancelEventArgs e) { Commit(); base.OnValidating(e); }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Up or Keys.Down) { Commit(); Assign(_value + (e.KeyCode == Keys.Up ? Increment : -Increment)); SelectAll(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Enter) { Commit(); SelectAll(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { FormatValue(); SelectAll(); e.SuppressKeyPress = true; }
        base.OnKeyDown(e);
    }
}
