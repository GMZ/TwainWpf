﻿using System;
using System.Drawing;

namespace TwainWpf
{
    public class TransferImageEventArgs : EventArgs
    {
        public Bitmap Image { get; private set; }
        public bool ContinueScanning { get; set; }

        public TransferImageEventArgs(Bitmap image, bool continueScanning)
        {
            Image = image;
            ContinueScanning = continueScanning;
        }
    }
}
