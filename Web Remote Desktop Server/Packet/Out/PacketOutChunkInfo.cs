using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutChunkInfo : IPacket
    {
        public byte PacketId => 0;
        private readonly byte chunkType;

        public PacketOutChunkInfo(byte chunkType)
        {
            this.chunkType = chunkType;
        }

        public ByteBuf Read(ByteBuf buf)
        {
            throw new NotImplementedException();
        }

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.WriteByte(chunkType);
            return buf;
        }
    }
}
