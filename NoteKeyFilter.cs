using System.Runtime.InteropServices;

namespace Stroik;

// Native ComboBox arrows are input keys and can bypass Form.ProcessCmdKey,
// particularly while its separate dropdown-list window has keyboard focus.
internal sealed class NoteKeyFilter(ComboBox target, Func<Keys, bool> navigate) : IMessageFilter
{
    public bool PreFilterMessage(ref Message message)
    {
        if (message.Msg != 0x0100 || !target.IsHandleCreated || target.IsDisposed) return false;
        Keys key = (Keys)message.WParam.ToInt32() | Control.ModifierKeys;
        if (key is not (Keys.Up or Keys.Down or Keys.Left or Keys.Right)) return false;
        var info = new ComboBoxInfo { Size = Marshal.SizeOf<ComboBoxInfo>() };
        if (!GetComboBoxInfo(target.Handle, ref info)) return false;
        if (message.HWnd != target.Handle && message.HWnd != info.Item && message.HWnd != info.List) return false;
        return navigate(key);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ComboBoxInfo
    {
        public int Size;
        public Rectangle ItemRectangle, ButtonRectangle;
        public int ButtonState;
        public IntPtr Combo, Item, List;
    }
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetComboBoxInfo(IntPtr window, ref ComboBoxInfo info);
}
