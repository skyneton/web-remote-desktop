using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutImageFullScreen : IPacket
    {
        public byte PacketId => 1;
        private readonly int width, height;
        private readonly byte[] image;
        public PacketOutImageFullScreen(int width, int height, byte[] image)
        {
            this.width = width;
            this.height = height;
            this.image = image;
        }

        public ByteBuf Read(ByteBuf buf)
        {
            throw new NotImplementedException();
        }

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.WriteVarInt(width);
            buf.WriteVarInt(height);
            buf.Write(image);
            return buf;
        }
    }
}
