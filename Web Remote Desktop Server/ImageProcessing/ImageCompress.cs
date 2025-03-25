using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WebRemoteDesktopServer.ImageProcessing
{
    static class ImageCompress
    {
        public static byte[] PixelToImage(int width, int height, PixelFormat format, int pixelPer, byte[] data, ImageFormat imageFormat)
        {
            using var image = new Bitmap(width, height, format);
            var bitmapData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);
            var stride = bitmapData.Stride;
            var ptr = bitmapData.Scan0;
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(data, y * stride, ptr, width * pixelPer);
                ptr += stride;
            }
            image.UnlockBits(bitmapData);
            var ms = new MemoryStream();
            image.Save(ms, imageFormat);

            return ms.ToArray();
        }

        public static byte[] PixelToImage(Bitmap image, ImageFormat imageFormat)
        {
            var ms = new MemoryStream();
            image.Save(ms, imageFormat);

            return ms.ToArray();
        }
    }
}
