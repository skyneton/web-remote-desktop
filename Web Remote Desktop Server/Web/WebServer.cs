using System.Net;

namespace WebRemoteDesktopServer.Web
{
    class WebServer
    {
        private readonly HttpListener listener = new();
        internal Dictionary<string, IPage> Pages { get; private set; } = [];
        public WebServer(int port = 80) : this("http://*", port) { }
        public WebServer(string domain, int port = 80)
        {
            ServicePointManager.MaxServicePointIdleTime = int.MaxValue;
            listener.TimeoutManager.MinSendBytesPerSecond = 0;
            listener.Prefixes.Add($"{domain}:{port}/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
            listener.BeginGetContext(WebRequestCallback, null);
        }

        public void Close()
        {
            listener.Close();
            PacketWebSocket.Close();
        }

        private void WebRequestCallback(IAsyncResult ar)
        {
            if (listener == null || !listener.IsListening) return;
            try
            {
                var context = listener.EndGetContext(ar);
                if (context != null)
                {
                    if (context.Request.IsWebSocketRequest)
                    {
                        context.Response.KeepAlive = true;
                        context.Response.AddHeader("Cross-Origin-Embedder-Policy", "require-corp");
                        context.Response.AddHeader("Cross-Origin-Opener-Policy", "same-origin");
                        PacketWebSocket.WebSocketHandler(context);
                    }
                    else
                    {
                        Pages.TryGetValue(context.Request.Url.AbsolutePath, out var page);
                        if (page != null)
                        {
                            context.Response.AddHeader("Cross-Origin-Embedder-Policy", "require-corp");
                            context.Response.AddHeader("Cross-Origin-Opener-Policy", "same-origin");
                            page.Write(context.Response);
                        }
                        else
                            context.Response.StatusCode = 404;

                        context.Response.Close();
                    }
                }
            }
            catch (Exception) { }
            try
            {
                listener.BeginGetContext(WebRequestCallback, null);
            }
            catch (Exception) { }
        }
    }
}
