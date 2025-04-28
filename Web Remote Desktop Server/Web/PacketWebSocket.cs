using System.Drawing.Imaging;
using System.Net;
using System.Net.WebSockets;
using WebRemoteDesktopServer.Capture;
using WebRemoteDesktopServer.ImageProcessing;
using WebRemoteDesktopServer.Packet;
using WebRemoteDesktopServer.Packet.Out;
using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Web
{
    class PacketWebSocket
    {
        public static int MaxSocketIdx { get; private set; } = 3;
        private static HashSet<PacketWebSocket>[] Sockets;
        public static int Count => Sockets[0].Count;
        private readonly WebSocket socket;
        private readonly int socketIdx;
        private readonly SemaphoreSlim sendSemaphore = new(1, 1);
        public WebSocketState State => socket.State;

        static PacketWebSocket()
        {
            ChangeSocketAmount(3);
        }

        public static void ChangeSocketAmount(int amount)
        {
            MaxSocketIdx = Math.Max(1, amount);
            Sockets = new HashSet<PacketWebSocket>[MaxSocketIdx];
            for (var i = 0; i < MaxSocketIdx; i++)
            {
                Sockets[i] = [];
            }
        }

        private PacketWebSocket(WebSocket socket, int idx)
        {
            this.socket = socket;
            socketIdx = idx;
        }

        public static async Task WebSocketHandler(HttpListenerContext context)
        {
            var socket = (await context.AcceptWebSocketAsync(null))?.WebSocket;
            int.TryParse(context.Request.Url.AbsolutePath.Substring(1), out var idx);
            if (socket?.State != WebSocketState.Open) return;
            idx %= MaxSocketIdx;
            var ws = new PacketWebSocket(socket, idx);
            if (!Worker.Password.Equals(await ws.ReceiveStringAsync()))
            {
                await ws.Dispose();
                return;
            }
            if (idx == 0)
            {
                SetResolution(ws);
                await ws.SendPacket(new PacketOutChunkInfo((byte)Worker.CurrentImageProcess.Quality));
                SendScreen(ws);
                await ws.SendPacket(new PacketOutCursorType(Worker.CursorInfo));
            }
            lock (Sockets)
            {
                Sockets[idx].Add(ws);
            }
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var packet = await ws.ReceiveAsync();
                    if (packet != null)
                        ws.ReceivePacket(new ByteBuf(packet));
                }
                catch (Exception)
                { }
                //TODO: Handle Packet
            }
            lock (Sockets)
            {
                Sockets[idx].Remove(ws);
            }
            if (Count <= 0) Worker.ReleaseResolution();
        }

        public static void Close()
        {
            foreach (var sockets in Sockets)
            {
                foreach (var socket in sockets)
                {
                    socket.Dispose();
                }
                sockets.Clear();
            }
        }

        //private static bool TryGetKeyCode(string code, string key, out int keyCode)
        //{
        //    Enum.TryParse(typeof(Keys), code, )
        //

        private void ReceivePacket(ByteBuf packet)
        {
            switch (packet.ReadVarInt())
            {
                case 0:
                    var key = packet.ReadVarInt();
                    var flags = (int)LowBinder.KeyType.KeyDown;
                    if (Keyboard.IsExtendedKey((Keys)key))
                        flags |= (int)LowBinder.KeyType.ExtendedKey;
                    LowBinder.keybd_event(key, 0, flags, 0);
                    break;
                case 1:
                    key = packet.ReadVarInt();
                    if (Keyboard.IsExtendedKey((Keys)key))
                    {
                        LowBinder.keybd_event(key, 0, (int)LowBinder.KeyType.KeyUp | (int)LowBinder.KeyType.ExtendedKey, 0);
                    }
                    LowBinder.keybd_event(key, 0, (int)LowBinder.KeyType.KeyUp, 0);
                    break;
                case 2:
                    var x = packet.ReadVarInt();
                    var y = packet.ReadVarInt();
                    LowBinder.SetCursorPos(x, y);
                    break;
                case 3:
                    x = packet.ReadVarInt();
                    y = packet.ReadVarInt();
                    var button = packet.ReadVarInt();
                    LowBinder.SetCursorPos(x, y);
                    switch (button)
                    {
                        case 0:
                            LowBinder.mouse_event((int)LowBinder.MouseType.LeftButtonDown, 0, 0, 0, 0);
                            break;
                        case 1:
                            LowBinder.mouse_event((int)LowBinder.MouseType.MiddleDown, 0, 0, 0, 0);
                            break;
                        case 2:
                            LowBinder.mouse_event((int)LowBinder.MouseType.RightButtonDown, 0, 0, 0, 0);
                            break;
                        case 3:
                            LowBinder.mouse_event((int)LowBinder.MouseType.XButtonDown, 0, 0, 0x1, 0);
                            break;
                        case 4:
                            LowBinder.mouse_event((int)LowBinder.MouseType.XButtonDown, 0, 0, 0x2, 0);
                            break;
                    }
                    break;
                case 4:
                    x = packet.ReadVarInt();
                    y = packet.ReadVarInt();
                    button = packet.ReadVarInt();
                    LowBinder.SetCursorPos(x, y);
                    switch (button)
                    {
                        case 0:
                            LowBinder.mouse_event((int)LowBinder.MouseType.LeftButtonUp, 0, 0, 0, 0);
                            break;
                        case 1:
                            LowBinder.mouse_event((int)LowBinder.MouseType.MiddleUp, 0, 0, 0, 0);
                            break;
                        case 2:
                            LowBinder.mouse_event((int)LowBinder.MouseType.RightButtonUp, 0, 0, 0, 0);
                            break;
                        case 3:
                            LowBinder.mouse_event((int)LowBinder.MouseType.XButtonUp, 0, 0, 0x1, 0);
                            break;
                        case 4:
                            LowBinder.mouse_event((int)LowBinder.MouseType.XButtonUp, 0, 0, 0x2, 0);
                            break;
                    }
                    break;
                case 5:
                    x = packet.ReadVarInt();
                    y = packet.ReadVarInt();
                    var wheel = packet.ReadVarInt();
                    LowBinder.SetCursorPos(x, y);
                    LowBinder.mouse_event((int)LowBinder.MouseType.Wheel, 0, 0, wheel, 0);
                    break;
            }
        }

        private static async void SetResolution(PacketWebSocket socket)
        {
            var raw = await socket.ReceiveAsync();
            if (raw == null) return;
            var buf = new ByteBuf(raw);
            var width = buf.ReadVarInt();
            var height = buf.ReadVarInt();
            if (Count <= 0) Worker.SetResolution(width, height);
        }

        private static async void SendScreen(PacketWebSocket socket)
        {
            await Task.Delay(15);
            var info = DisplaySettings.GetResolution();
            using var screen = ScreenCapture.Screenshot(info.Width, info.Height, Worker.CurrentImageProcess.Format);
            socket.SendPacket(new PacketOutImageFullScreen(ImageCompress.PixelToImage(screen, ImageFormat.Jpeg)));
        }

        public Task? SendPacket(IPacket packet)
        {
            var buffer = packet.Write(new ByteBuf()).Flush();
            if (socket.State != WebSocketState.Open) return null;
            return SendBinary(buffer);
        }

        private async Task SendBinary(byte[] buffer)
        {
            await sendSemaphore.WaitAsync();
            try
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            finally { sendSemaphore.Release(); }
        }

        public static List<Task> Broadcast(IPacket packet, int idx)
        {
            var buffer = packet.Write(new ByteBuf()).Flush();
            var list = new List<Task>();
            foreach (var socket in Sockets[idx])
            {
                if (socket.State != WebSocketState.Open) continue;
                list.Add(socket.SendBinary(buffer));
            }
            return list;
        }

        private Task Dispose(WebSocketCloseStatus status = WebSocketCloseStatus.InvalidMessageType, string? message = null)
        {
            return socket.CloseAsync(status, message, CancellationToken.None);
        }

        private async Task<string?> ReceiveStringAsync()
        {
            var raw = await ReceiveAsync();
            return raw == null ? null : new ByteBuf(raw).ReadString();
        }

        private async Task<byte[]?> ReceiveAsync()
        {
            if (socket.State != WebSocketState.Open) return null;
            {
                var buffer = new byte[1024];
                var receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (receiveResult?.MessageType != WebSocketMessageType.Binary)
                {
                    await Dispose();
                    return null;
                }
                var idx = receiveResult.Count;
                while (!receiveResult.EndOfMessage)
                {
                    if (idx >= buffer.Length)
                    {
                        await Dispose(WebSocketCloseStatus.MessageTooBig);
                        return null;
                    }
                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, idx, buffer.Length - idx), CancellationToken.None);
                    idx += receiveResult.Count;
                }
                return buffer[0..idx];
            }
        }
    }
}
