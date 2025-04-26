using NAudio.Wave;
using WebRemoteDesktopServer.Packet.Out;
using WebRemoteDesktopServer.Web;

namespace WebRemoteDesktopServer.Capture
{
    class SoundCapture
    {
        private readonly WasapiLoopbackCapture capture = new();
        public SoundCapture(int sampleRate, int bitsPerSample, int channels)
        {
            capture.WaveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
            capture.DataAvailable += (s, a) =>
            {
                PacketWebSocket.Broadcast(new PacketOutSoundData((byte)capture.WaveFormat.Channels, capture.WaveFormat.SampleRate, capture.WaveFormat.BitsPerSample, a.Buffer), PacketWebSocket.MaxSocketIdx - 1);
            };
            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };
        }

        ~SoundCapture() { Close(); }

        public void Start()
        {
            capture.StartRecording();
        }

        public void Close() { capture.StopRecording(); }
    }
}
