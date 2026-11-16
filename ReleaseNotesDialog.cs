using System.Drawing;

namespace Stroik;

internal sealed class ReleaseNotesDialog : Form
{
    internal ReleaseNotesDialog(Version version, string notes, bool english, bool dark, bool offerUpdate)
        : this(version, notes, english ? "en" : "pl", dark, offerUpdate) { }
    internal ReleaseNotesDialog(Version version, string notes, string language, bool dark, bool offerUpdate)
    {
        string T(string pl, string en) => UiLanguages.Get(language, pl, en);
        string Tf(FormattableString pl, FormattableString en) => UiLanguages.Format(language, pl, en);
        Text = offerUpdate
            ? T("Nowa wersja Stroika", "New Stroik version")
            : T("Ostatnie zmiany", "Latest changes");
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimumSize = new Size(460, 340);
        Size = new Size(650, 500);
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(16) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(layout);

        var introduction = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = offerUpdate
                ? Tf($"Dostępna jest wersja {version}. Zmiany są poniżej.\nWybierz Tak, aby pobrać wersję, podmienić EXE i uruchomić Stroika ponownie.", $"Version {version} is available. Changes are below.\nChoose Yes to download it, replace the EXE and restart Stroik.")
                : Tf($"Zmiany w wersji {version}:", $"Changes in version {version}:"),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(introduction, 0, 0);

        var changes = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            WordWrap = true,
            ScrollBars = ScrollBars.Vertical,
            TabStop = true,
            Text = notes,
            AccessibleName = Tf($"Zmiany w wersji {version}", $"Changes in version {version}")
        };
        layout.Controls.Add(changes, 0, 1);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0)
        };
        layout.Controls.Add(actions, 0, 2);

        if (offerUpdate)
        {
            var no = new Button
            {
                AutoSize = true,
                DialogResult = DialogResult.No,
                Text = T("Nie", "No")
            };
            var yes = new Button
            {
                AutoSize = true,
                DialogResult = DialogResult.Yes,
                Text = T("Tak, pobierz i zainstaluj", "Yes, download and install")
            };
            actions.Controls.Add(no);
            actions.Controls.Add(yes);
            AcceptButton = no;
            CancelButton = no;
        }
        else
        {
            var close = new Button
            {
                AutoSize = true,
                DialogResult = DialogResult.OK,
                Text = T("Zamknij", "Close")
            };
            actions.Controls.Add(close);
            AcceptButton = close;
            CancelButton = close;
        }

        if (dark)
        {
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.WhiteSmoke;
            layout.BackColor = BackColor;
            introduction.ForeColor = ForeColor;
            changes.BackColor = Color.FromArgb(48, 48, 48);
            changes.ForeColor = ForeColor;
            actions.BackColor = BackColor;
            foreach (Button button in actions.Controls) { button.BackColor = Color.FromArgb(48, 48, 48); button.ForeColor = ForeColor; }
        }
        Shown += (_, _) => { changes.SelectionStart = 0; changes.SelectionLength = 0; changes.Focus(); };
    }
}
