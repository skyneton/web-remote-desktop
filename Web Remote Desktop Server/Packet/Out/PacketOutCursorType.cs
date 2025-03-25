using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutCursorType : IPacket
    {
        public byte PacketId => 3;
        private readonly int cursor;
        public PacketOutCursorType(int cursor)
        {
            this.cursor = cursor;
        }

        public ByteBuf Read(ByteBuf buf)
        {
            throw new NotImplementedException();
        }

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.WriteVarInt(cursor);
            return buf;
        }
    }
}
