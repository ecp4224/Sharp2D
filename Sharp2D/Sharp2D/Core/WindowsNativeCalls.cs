using System;
using System.Runtime.InteropServices;
using Sharp2D.Core.Interfaces;

namespace Sharp2D.Core
{
    public class WindowsNativeSystem : INativeSystem
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SwHide = 0;
        private const int SwShow = 5;


        public bool ToggleConsoleWindow(bool show)
        {
            var handle = GetConsoleWindow();

            return ShowWindow(handle, show ? SwShow : SwHide);
        }

        public string SystemName
        {
            get { return Environment.OSVersion.ToString(); }
        }
    }
}