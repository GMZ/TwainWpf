using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TwainWpf.Win32
{
	public class BitmapRenderer : IDisposable
	{
		private readonly IntPtr _dibHandle;
		private readonly IntPtr _bitmapPointer;
		private readonly IntPtr _pixelInfoPointer;
        private Rectangle _rectangle;
	    private readonly BitmapInfoHeader _bitmapInfo;

		public BitmapRenderer(IntPtr dibHandle)
		{
			_dibHandle = dibHandle;
			_bitmapPointer = Kernel32Native.GlobalLock(dibHandle);

			_bitmapInfo = new BitmapInfoHeader();
			Marshal.PtrToStructure(_bitmapPointer, _bitmapInfo);

            _rectangle = new Rectangle();
            _rectangle.X = _rectangle.Y = 0;
            _rectangle.Width = _bitmapInfo.Width;
            _rectangle.Height = _bitmapInfo.Height;

			if (_bitmapInfo.SizeImage == 0)
			{
                _bitmapInfo.SizeImage = ((_bitmapInfo.Width * _bitmapInfo.BitCount + 31 & ~31) >> 3) * _bitmapInfo.Height;
            }

            // The following code only works on x86
            Debug.Assert(Marshal.SizeOf(typeof(IntPtr)) == 4);

            int pixelInfoPointer = _bitmapInfo.ClrUsed;
			if (pixelInfoPointer == 0 && _bitmapInfo.BitCount <= 8)
			{
			    pixelInfoPointer = 1 << (_bitmapInfo.BitCount); // & 31);
			}
			pixelInfoPointer = pixelInfoPointer * 4 + _bitmapInfo.Size + _bitmapPointer.ToInt32();

			_pixelInfoPointer = new IntPtr(pixelInfoPointer);
		}

        ~BitmapRenderer()
        {
            Dispose(false);
        }

        public Bitmap RenderToBitmap()
        {
            var bitmap = new Bitmap(_rectangle.Width, _rectangle.Height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                var hdc = graphics.GetHdc();

                try
                {
                    Gdi32Native.SetDIBitsToDevice(hdc, 0, 0, _rectangle.Width, _rectangle.Height,
                        0, 0, 0, _rectangle.Height, _pixelInfoPointer, _bitmapPointer, 0);
                }
                finally
                {
                    graphics.ReleaseHdc(hdc);
                }
            }

            bitmap.SetResolution(PpmToDpi(_bitmapInfo.XPelsPerMeter), PpmToDpi(_bitmapInfo.YPelsPerMeter));

            return bitmap;
        }

        private static float PpmToDpi(double pixelsPerMeter)
        {
            var pixelsPerMillimeter = pixelsPerMeter / 1000.0;
            var dotsPerInch = pixelsPerMillimeter * 25.4;
            return (float)Math.Round(dotsPerInch, 2);
        }

        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Kernel32Native.GlobalUnlock(_dibHandle);
			Kernel32Native.GlobalFree(_dibHandle);
		}
	}
}