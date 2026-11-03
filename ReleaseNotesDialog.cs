using System.Drawing;

namespace Stroik;

internal sealed class ReleaseNotesDialog : Form
{
    internal ReleaseNotesDialog(Version version, string notes, bool english, bool dark, bool offerUpdate)
    {
        Text = offerUpdate
            ? (english ? "New Stroik version" : "Nowa wersja Stroika")
            : (english ? "Latest changes" : "Ostatnie zmiany");
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
            AutoSize = true, Dock = DockStyle.Fill,
            Text = offerUpdate
                ? (english ? $"Version {version} is available. Changes are below.\nChoose Yes to download it, replace the EXE and restart Stroik."
                    : $"Dostępna jest wersja {version}. Zmiany są poniżej.\nWybierz Tak, aby pobrać wersję, podmienić EXE i uruchomić Stroika ponownie.")
                : (english ? $"Changes in version {version}:" : $"Zmiany w wersji {version}:"),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(introduction, 0, 0);

        var changes = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, WordWrap = true,
            ScrollBars = ScrollBars.Vertical, TabStop = true, Text = notes,
            AccessibleName = english ? $"Changes in version {version}" : $"Zmiany w wersji {version}"
        };
        layout.Controls.Add(changes, 0, 1);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0)
        };
        layout.Controls.Add(actions, 0, 2);

        if (offerUpdate)
        {
            var no = new Button { AutoSize = true, DialogResult = DialogResult.No,
                Text = english ? "No" : "Nie" };
            var yes = new Button { AutoSize = true, DialogResult = DialogResult.Yes,
                Text = english ? "Yes, download and install" : "Tak, pobierz i zainstaluj" };
            actions.Controls.Add(no);
            actions.Controls.Add(yes);
            AcceptButton = no;
            CancelButton = no;
        }
        else
        {
            var close = new Button { AutoSize = true, DialogResult = DialogResult.OK,
                Text = english ? "Close" : "Zamknij" };
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
