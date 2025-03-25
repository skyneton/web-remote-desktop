using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutImageChunk : IPacket
    {
        public byte PacketId => 2;
        private readonly byte[] pixelPos;
        private readonly bool isCompress;
        private readonly byte[] pixelData;

        public PacketOutImageChunk(byte[] pixelPos, bool isCompress, byte[] pixelData)
        {
            this.pixelPos = pixelPos;
            this.isCompress = isCompress;
            this.pixelData = pixelData;
        }

        public ByteBuf Read(ByteBuf buf)
        {
            throw new NotImplementedException();
        }

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.WriteBool(isCompress);
            buf.WriteVarInt(pixelData.Length);
            buf.Write(pixelData);
            buf.Write(pixelPos);
            return buf;
        }
    }
}
