using System.Runtime.InteropServices;
using System.Text;
using WindowsInput;
using WindowsInput.Native;

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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_RESTORE = 9;
        const int SW_MINIMIZE = 6;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int VK_CONTROL = 0x11;

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
                        SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_CONTROL, IntPtr.Zero);
                        await Task.Delay(100, stoppingToken);
                        SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_CONTROL, IntPtr.Zero);
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
