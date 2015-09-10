using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Lib
{
    public static class BitmapConverter
    {
        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
                (
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            return bitmapSource;
        }

        public static Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap
                (
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb
                );
            var data = bmp.LockBits
                (
                    new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppPArgb
                );
            source.CopyPixels
                (
                    Int32Rect.Empty,
                    data.Scan0,
                    data.Height*data.Stride,
                    data.Stride
                );
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
