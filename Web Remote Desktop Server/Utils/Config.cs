namespace WebRemoteDesktopServer.Utils
{
    public class Config
    {
        public string Password { get; set; } = "";
        public string Address { get; set; } = "http://*";
        public int Port { get; set; } = 80;
        public int SocketAmount { get; set; } = 3;
        public AudioConfig Audio { get; set; } = new AudioConfig();
        public int ImageQuality { get; set; } = 0;
        public int FramePerSecond { get; set; } = 30;

        public class AudioConfig
        {
            public bool Allow { get; set; } = true;
            public int SampleRate { get; set; } = 44100;
            public int BitsPerSample { get; set; } = 16;
            public int Channels { get; set; } = 2;
        }
    }
}
