using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutImageFullScreen : IPacket
    {
        public byte PacketId => 1;
        private readonly byte[] image;
        public PacketOutImageFullScreen(byte[] image)
        {
            this.image = image;
        }

        public ByteBuf Read(ByteBuf buf)
        {
            throw new NotImplementedException();
        }

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.Write(image);
            return buf;
        }
    }
}
