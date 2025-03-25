using WebRemoteDesktopServer.Utils;

namespace WebRemoteDesktopServer.Packet.Out
{
    class PacketOutSoundData : IPacket
    {
        public byte PacketId => 4;
        private readonly byte channels;
        private readonly int sampleRate;
        private readonly int bitsPerSample;
        private readonly byte[] sound;
        public PacketOutSoundData(byte channels, int sampleRate, int bitsPerSample, byte[] sound)
        {
            this.channels = channels;
            this.sampleRate = sampleRate;
            this.bitsPerSample = bitsPerSample;
            this.sound = sound;
        }

        public ByteBuf Read(ByteBuf buf) => throw new NotImplementedException();

        public ByteBuf Write(ByteBuf buf)
        {
            buf.WriteByte(PacketId);
            buf.WriteByte(channels);
            buf.WriteVarInt(sampleRate);
            buf.WriteVarInt(bitsPerSample);
            buf.Write(sound);
            return buf;
        }
    }
}
