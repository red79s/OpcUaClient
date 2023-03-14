using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Newtonsoft.Json;

namespace OpcUaClient
{
    public class WindowPlacement
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        public struct POINTAPI
        {
            public int x;
            public int y;
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINTAPI ptMinPosition;
            public POINTAPI ptMaxPosition;
            public RECT rcNormalPosition;
        }

        public static void RestoreWindowPosition()
        {
            AppSettingsManager manager = new AppSettingsManager();
            var wpStr = manager.Get("WindowPlacement");
            if (string.IsNullOrEmpty(wpStr))
                return;

            WINDOWPLACEMENT wp = JsonConvert.DeserializeObject<WINDOWPLACEMENT>(wpStr);
            IntPtr appHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            SetWindowPlacement(appHandle, ref wp);
        }

        public static void StoreWindowPlacement()
        {
            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            IntPtr appHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            GetWindowPlacement(appHandle, ref wp);

            string wpStr = JsonConvert.SerializeObject(wp);
            AppSettingsManager manager = new AppSettingsManager();
            manager.Set("WindowPlacement", wpStr);
        }
    }
}
