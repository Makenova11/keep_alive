using System.Runtime.InteropServices;
using System.Text;

namespace keep_alive
{
    public class WindowsWorker : BackgroundService
    {
        private readonly string _remoteDesktopWindowName;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_RESTORE = 9;
        const int SW_MINIMIZE = 6;
        public WindowsWorker(IConfiguration configuration)
        {
            _remoteDesktopWindowName = configuration["RemoteDesktopWindowName"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sim = new InputSimulator();
            while (!stoppingToken.IsCancellationRequested)
            {
                var hWnd = FindWindowByPartialName(_remoteDesktopWindowName ?? "vpn-nsk1.global.bcs");
                if (hWnd != IntPtr.Zero)
                {
                    if (IsIconic(hWnd))
                    {
                        //SetForegroundWindow(hWnd);
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);

                        await Task.Delay(300, stoppingToken);

                        sim.Keyboard.KeyPress(VirtualKeyCode.CONTROL);
                        ShowWindow(hWnd, SW_MINIMIZE);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        static IntPtr FindWindowByPartialName(string partialName)
        {
            IntPtr foundWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                var windowText = new StringBuilder(256);
                GetWindowText(hWnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains(partialName))
                {
                    foundWindow = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return foundWindow;
        }
    }
}
