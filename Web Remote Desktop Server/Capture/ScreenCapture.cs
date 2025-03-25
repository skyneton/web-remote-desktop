using System.Drawing;
using System.Drawing.Imaging;
using WebRemoteDesktopServer.ImageProcessing;
using WebRemoteDesktopServer.Packet.Out;
using WebRemoteDesktopServer.Utils;
using WebRemoteDesktopServer.Web;

namespace WebRemoteDesktopServer.Capture
{
    static class ScreenCapture
    {
        private const int ImageThreading = 32767; // maximum gdi+ jpeg width
        internal static int beforeWidth, beforeHeight;
        internal static byte[] pixelData = [];
        internal static void Run(ImageProcess process)
        {
            if (PacketWebSocket.Count <= 0) return;
            var info = DisplaySettings.GetResolution();
            //Console.WriteLine($"{info.Width} x {info.Height}");
            using var screen = Screenshot(info.Width, info.Height, process.Format);

            var bitmapData = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height), ImageLockMode.ReadOnly, process.Format);
            var force = info.Width != beforeWidth || info.Height != beforeHeight;
            if (force)
                pixelData = new byte[info.Width * info.Height * process.PixelBytes];

            beforeWidth = info.Width;
            beforeHeight = info.Height;

            var list = new List<Task>();
            var idx = 0;
            var packetAmount = PacketWebSocket.MaxSocketIdx;
            if (Worker.AllowAudio) packetAmount = Math.Max(1, packetAmount - 1);
            for (int i = 0, end = info.Width * info.Height; i < end; i += ImageThreading)
            {
                var start = i;
                var key = (idx++) % packetAmount;
                list.Add(Task.Run(() =>
                {
                    var packet = process.Process(bitmapData, pixelData, start, Math.Min(end, start + ImageThreading), force);
                    if (packet == null) return;
                    PacketWebSocket.Broadcast(packet, key);
                }));
            }
            Task.WaitAll(list);
            screen.UnlockBits(bitmapData);

            if (force)
                PacketWebSocket.Broadcast(new PacketOutImageFullScreen(ImageCompress.PixelToImage(screen, ImageFormat.Jpeg)), 0);

            var cursorInfo = LowBinder.GetCursorInfo(out var success);
            if (success && Worker.CursorInfo != cursorInfo.hCursor)
            {
                Worker.CursorInfo = (int)cursorInfo.hCursor;
                PacketWebSocket.Broadcast(new PacketOutCursorType(Worker.CursorInfo), 0);
            }
        }
        internal static Bitmap Screenshot(int width, int height, PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);
            DisplaySettings.CopyFromScreen(bitmap);
            //Console.WriteLine($"{VirtualDesktop.GetCurrentDesktop()} {IntPtr.Zero}");
            //using var g = Graphics.FromImage(bitmap);
            //g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
            return bitmap;
        }
    }
}
