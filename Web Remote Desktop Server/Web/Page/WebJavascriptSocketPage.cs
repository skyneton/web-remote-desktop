using System.Net;
using System.Text;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptSocketPage : IPage
    {
        private readonly byte[] page;
        public WebJavascriptSocketPage(int socketCount)
        {
            page = Encoding.UTF8.GetBytes(
                Encoding.UTF8.GetString(Properties.Resources.JSSocket)
                .Replace("%{SOCKET_COUNT}%", socketCount.ToString()));
        }
        public void Write(HttpListenerResponse response)
        {
            response.ContentLength64 = page.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(page);
        }
    }
}
