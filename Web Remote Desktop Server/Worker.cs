using System.Text.Json;
using WebRemoteDesktopServer.Capture;
using WebRemoteDesktopServer.ImageProcessing;
using WebRemoteDesktopServer.Utils;
using WebRemoteDesktopServer.Web;
using WebRemoteDesktopServer.Web.Page;

namespace WebRemoteDesktopServer
{
    public class Worker
    {
        internal static string Password = "";
        internal static ImageProcess CurrentImageProcess { get; private set; } = ImageProcess.Byte2RGB;
        internal static DisplaySettings.ResolutionInfo BaseResolutionInfo { get; private set; } = DisplaySettings.GetResolution();
        internal static int CursorInfo = 65539;
        private static WebServer server;
        private static SoundCapture soundCapture;
        internal static bool AllowAudio { get; private set; }
        private static int framePerSecond;

        ~Worker() { ReleaseResolution(); }

        public void Execute()
        {
            LoadConfig();
            if(AllowAudio) soundCapture.Start();
            while (true)
            {
                try
                {
                    ScreenCapture.Run(CurrentImageProcess);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Thread.Sleep(1000 / framePerSecond);
            }
        }

        private static void LoadConfig()
        {
            if (!File.Exists("./config.json"))
                File.WriteAllBytes("./config.json", Properties.Resources.Config);

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json")) ?? new Config();
            Password = config.Password;

            server = new WebServer(config.Address, config.Port);
            server.Pages.Add("/", new WebHtmlIndexPage());
            server.Pages.Add("/bytebuf.js", new WebJavascriptByteBufPage());
            server.Pages.Add("/main.js", new WebJavascriptMainPage());
            server.Pages.Add("/socket.js", new WebJavascriptSocketPage(config.SocketAmount));
            server.Pages.Add("/style.css", new WebCssStylePage());

            PacketWebSocket.ChangeSocketAmount(config.SocketAmount);

            AllowAudio = config.Audio.Allow;
            soundCapture = new SoundCapture(config.Audio.SampleRate, config.Audio.BitsPerSample, config.Audio.Channels);

            CurrentImageProcess = config.ImageQuality switch
            {
                0 => ImageProcess.Byte3RGB,
                _ => ImageProcess.Byte2RGB,
            };

            framePerSecond = config.FramePerSecond;
        }

        public static void SetResolution(int width, int height)
        {
            DisplaySettings.ChangeDisplayResolution(width, height);
        }

        public static void ReleaseResolution()
        {
            DisplaySettings.ChangeDisplayResolution(BaseResolutionInfo.Width, BaseResolutionInfo.Height);
        }
    }
}
