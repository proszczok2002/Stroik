using System.Globalization;

namespace Stroik;

// Accept either decimal separator, and commit once on Enter/focus loss/value access.
// Mark the edit committed before ValueChanged handlers persist the form to avoid recursive parsing.
internal sealed class EditableNumber : NumericUpDown
{
    protected override void ValidateEditText()
    {
        if (!UserEdit) return;
        string input = Text.Trim().Replace(',', '.');
        UserEdit = false;
        if (decimal.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out decimal number))
            Value = Math.Clamp(decimal.Round(number, DecimalPlaces, MidpointRounding.AwayFromZero), Minimum, Maximum);
        UpdateEditText();
    }
}
