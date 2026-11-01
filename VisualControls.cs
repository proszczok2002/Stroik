using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Stroik;

internal static class ScreenReaderDetector
{
    // SPI_GETSCREENREADER is incomplete (Windows Narrator does not set it).
    // Known process names are a best-effort supplement, never a replacement for the manual option.
    internal static bool IsActive()
    {
        if (SystemParametersInfo(0x0046, 0, out bool active, 0) && active) return true;
        foreach (string name in new[] { "nvda", "Narrator", "jfw", "jfw64" })
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(name);
                bool found = processes.Length > 0;
                foreach (Process process in processes) process.Dispose();
                if (found) return true;
            }
            catch (Exception error) when (error is System.ComponentModel.Win32Exception or InvalidOperationException) { }
        }
        return false;
    }

    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint param,
        [MarshalAs(UnmanagedType.Bool)] out bool result, uint flags);
}

internal sealed class PitchMeter : Control
{
    private double? _cents;
    private bool _english;
    internal void SetLanguage(bool english) { _english = english; Invalidate(); }

    internal PitchMeter()
    {
        Height = 92;
        MinimumSize = new Size(220, 92);
        DoubleBuffered = true;
        TabStop = false;
    }

    internal void SetReading(double? cents)
    {
        _cents = cents;
        if (Visible) Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float left = 25, right = Width - 25, center = (left + right) / 2;
        float y = Height * 0.55f;
        using var axis = new Pen(ForeColor, 2);
        using var good = new Pen(Color.ForestGreen, 6);
        using var marker = new Pen(_cents is >= -5 and <= 5 ? Color.LimeGreen : Color.OrangeRed, 5);
        e.Graphics.DrawLine(axis, left, y, right, y);
        e.Graphics.DrawLine(good, center - 8, y, center + 8, y);
        e.Graphics.DrawLine(axis, center, y - 17, center, y + 18);
        if (_cents is double cents)
        {
            float x = center + (float)Math.Clamp(cents / 50, -1, 1) * (right - left) / 2;
            e.Graphics.DrawLine(marker, x, y - 23, x, y + 23);
        }
        string caption = _cents is double value
            ? (_english ? $"Live: {value:+0.0;-0.0;0.0} cents" : $"Na żywo: {value:+0.0;-0.0;0.0} centów")
            : (_english ? "Play one note" : "Zagraj jeden dźwięk");
        TextRenderer.DrawText(e.Graphics, caption, Font, new Rectangle(5, 0, Width - 10, 30),
            ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(e.Graphics, "−50", Font, new Point((int)left - 10, Height - 22), ForeColor);
        TextRenderer.DrawText(e.Graphics, "+50", Font, new Point((int)right - 25, Height - 22), ForeColor);
    }
}

internal sealed class KeyboardDiagram : Control
{
    private int _midi = NoteCatalog.DefaultMidi;
    internal KeyboardDiagram()
    {
        Height = 110;
        MinimumSize = new Size(220, 110);
        DoubleBuffered = true;
        TabStop = false;
    }

    internal void SelectNote(int midi)
    {
        if (midi is >= NoteCatalog.LowestMidi and <= NoteCatalog.HighestMidi) _midi = midi;
        if (Visible) Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        int firstC = Math.Clamp(_midi / 12 * 12 - 12, 12, 96);
        float whiteWidth = (Width - 12f) / 14f;
        float top = 5, bottom = Height - 8;
        int[] whitePitches = [0, 2, 4, 5, 7, 9, 11];
        using var whiteBrush = new SolidBrush(Color.White);
        using var blackBrush = new SolidBrush(Color.FromArgb(24, 24, 24));
        using var outline = new Pen(Color.Gray);
        using var dot = new SolidBrush(Color.DeepSkyBlue);
        for (int octave = 0; octave < 2; octave++)
        for (int i = 0; i < 7; i++)
        {
            int midi = firstC + octave * 12 + whitePitches[i];
            float x = 6 + (octave * 7 + i) * whiteWidth;
            var rect = new RectangleF(x, top, whiteWidth, bottom - top);
            e.Graphics.FillRectangle(whiteBrush, rect);
            e.Graphics.DrawRectangle(outline, x, top, whiteWidth, bottom - top);
            if (midi == _midi)
                e.Graphics.FillEllipse(dot, x + whiteWidth / 2 - 6, bottom - 27, 12, 12);
        }
        int[] blackPitches = [1, 3, 6, 8, 10];
        int[] afterWhite = [0, 1, 3, 4, 5];
        for (int octave = 0; octave < 2; octave++)
        for (int i = 0; i < 5; i++)
        {
            int midi = firstC + octave * 12 + blackPitches[i];
            float width = whiteWidth * 0.62f;
            float x = 6 + (octave * 7 + afterWhite[i] + 1) * whiteWidth - width / 2;
            var rect = new RectangleF(x, top, width, (bottom - top) * 0.62f);
            e.Graphics.FillRectangle(blackBrush, rect);
            if (midi == _midi)
                e.Graphics.FillEllipse(dot, x + width / 2 - 5, top + 21, 10, 10);
        }
    }
}