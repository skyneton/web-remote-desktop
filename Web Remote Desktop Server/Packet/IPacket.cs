using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet
{
    interface IPacket
    {
        byte PacketId { get; }
        public ByteBuf Read(ByteBuf buf);
        public ByteBuf Write(ByteBuf buf);
    }
}
