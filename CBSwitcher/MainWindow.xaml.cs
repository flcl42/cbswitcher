using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CBSwitcher
{
    public partial class MainWindow : Window
    {

        public KeyBoardHook hook;

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardViewer();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        IntPtr viewerHandle = IntPtr.Zero;
        IntPtr installedHandle = IntPtr.Zero;

        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                installedHandle = hwndSource.Handle;
                viewerHandle = SetClipboardViewer(installedHandle);
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
            hook = new KeyBoardHook(hwndSource.Handle);
            hook.OnHotKey = () =>
            {               
                Visibility = System.Windows.Visibility.Visible;
            };
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ChangeClipboardChain(this.installedHandle, this.viewerHandle);
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            e.Cancel = error != 0;

            base.OnClosing(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            this.viewerHandle = IntPtr.Zero;
            this.installedHandle = IntPtr.Zero;
            base.OnClosed(e);

        }


        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_CHANGECBCHAIN:
                    this.viewerHandle = lParam;
                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

                case WM_DRAWCLIPBOARD:
                    EventArgs clipChange = new EventArgs();
                    OnClipboardChanged();

                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

            }
            return IntPtr.Zero;

        }

        public delegate void ClipboardChanged();
        public ClipboardChanged OnClipboardChanged = () => { };

        public MainWindow()
        {
           
            InitializeComponent();

            OnClipboardChanged = () =>
            {
                Data.Items.Add(new Item { Type = Clipboard.ContainsText() ? "Текст" : "Не текст", Data = Clipboard.ContainsText() ? Clipboard.GetText() : "<something else>", Title = (Clipboard.ContainsText() ? Clipboard.GetText() : "<something else>").Substring(0,50) });
            };
        }

        public class Item
        {
            public object Type { get; set; }
            public object Title { get; set; }
            public object Data { get; set; }
        }

        private void source_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }



    }
}
