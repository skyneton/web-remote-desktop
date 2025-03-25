using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WebRemoteDesktopServer.Packet.Out;
using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.ImageProcessing
{
    class ImageProcess
    {
        internal static readonly ImageProcess Byte3RGB = new();
        internal static readonly ImageProcess Byte2RGB = new ImageProcess565();
        virtual public QualityMode Quality => QualityMode.Byte3RGB;
        virtual public PixelFormat Format => PixelFormat.Format24bppRgb;
        virtual public byte PixelBytes => 3;


        public PacketOutImageChunk? Process(BitmapData bitmapData, byte[] colorData, int start, int end, bool force)
        {
            var offset = bitmapData.Stride - bitmapData.Width * (Image.GetPixelFormatSize(bitmapData.PixelFormat) >> 3);

            var height = start / bitmapData.Width;
            var width = start % bitmapData.Width;
            var idx = start;
            using var pixelPosStream = new MemoryStream();
            using var changedPixelStream = new MemoryStream();

            var ptr = bitmapData.Scan0;
            ptr += start * PixelBytes + height * offset;
            var startChanged = -1;
            if (width > 0)
            {
                var w = Math.Min(end - idx, bitmapData.Width - width);
                startChanged = Copy(pixelPosStream, changedPixelStream, startChanged, ptr, colorData, idx, w, force);
                idx += w;
                ptr += w * PixelBytes + offset;
            }
            while (idx < end)
            {
                var w = Math.Min(end - idx, bitmapData.Width);
                startChanged = Copy(pixelPosStream, changedPixelStream, startChanged, ptr, colorData, idx, w, force);
                idx += w;
                ptr += w * PixelBytes + offset;
            }

            if (startChanged >= 0)
            {
                pixelPosStream.Write(ByteBuf.GetVarInt(startChanged));
                //Console.WriteLine(startChanged);
                pixelPosStream.Write(ByteBuf.GetVarInt(end - startChanged));
            }

            if (pixelPosStream.Length <= 0) return null;
            var pixelPos = pixelPosStream.ToArray();
            var changedPixels = changedPixelStream.ToArray();
            var compressPixels = ImageCompress.PixelToImage(changedPixels.Length / PixelBytes, 1, Format, PixelBytes, changedPixels, ImageFormat.Png);

            if (compressPixels.Length < changedPixels.Length)
                return new PacketOutImageChunk(pixelPos, true, compressPixels);

            return new PacketOutImageChunk(pixelPos, false, changedPixels);
        }

        protected virtual int Copy(MemoryStream pixelPosStream, MemoryStream changedPixelStream, int startChanged, IntPtr ptr, byte[] pixelData, int offset, int amount, bool force)
        {
            if (force)
            {
                Marshal.Copy(ptr, pixelData, offset * PixelBytes, amount * PixelBytes);
                return -1;
            }
            for (int i = 0; i < amount; i++)
            {
                if (ConvertInput(changedPixelStream, ptr, pixelData, offset * PixelBytes))
                {
                    if (startChanged < 0) startChanged = offset;
                }
                else if (startChanged >= 0)
                {
                    pixelPosStream.Write(ByteBuf.GetVarInt(startChanged));
                    pixelPosStream.Write(ByteBuf.GetVarInt(offset - startChanged));
                    startChanged = -1;
                }
                offset++;
                ptr += PixelBytes;
            }
            return startChanged;
        }

        protected virtual bool ConvertInput(MemoryStream changedPixelStream, IntPtr ptr, byte[] pixelData, int offset)
        {
            var b = Marshal.ReadByte(ptr);
            ptr++;
            var g = Marshal.ReadByte(ptr);
            ptr++;
            var r = Marshal.ReadByte(ptr);
            if (pixelData[offset] != b || pixelData[offset + 1] != g || pixelData[offset + 2] != r)
            {
                pixelData[offset++] = b; pixelData[offset++] = g; pixelData[offset++] = r;
                changedPixelStream.WriteByte(b);
                changedPixelStream.WriteByte(g);
                changedPixelStream.WriteByte(r);
                return true;
            }
            return false;
        }
    }
}
