using System;
using System.Windows;
using System.Windows.Interop;

namespace TwainWpf.Wpf
{
    public class WindowMessageHook : IWindowsMessageHook
    {
        private readonly HwndSource _source;
        private readonly WindowInteropHelper _interopHelper;
        private bool _usingFilter;

        public FilterMessage FilterMessageCallback { get; set; }

        public bool UseFilter
        {
            get { return _usingFilter; }
            set
            {
                if (!_usingFilter && value)
                {
                    _source.AddHook(FilterMessage);
                    _usingFilter = true;
                }
                if (_usingFilter && !value)
                {
                    _source.RemoveHook(FilterMessage);
                    _usingFilter = false;
                }
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                return _interopHelper.Handle;
            }
        }

        public WindowMessageHook(Window window)
        {
            _source = (HwndSource)PresentationSource.FromDependencyObject(window);
            _interopHelper = new WindowInteropHelper(window);
        }

        public IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (FilterMessageCallback == null)
            {
                return IntPtr.Zero;
            }
            return FilterMessageCallback(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
