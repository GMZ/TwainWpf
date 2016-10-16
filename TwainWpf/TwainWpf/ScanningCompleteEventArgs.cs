using System;

namespace TwainWpf
{
    public class ScanningCompleteEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public ScanningCompleteEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
