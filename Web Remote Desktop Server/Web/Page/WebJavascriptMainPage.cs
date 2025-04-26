using System.Net;
using System.Text;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptMainPage : IPage
    {
        private readonly byte[] page;
        public WebJavascriptMainPage(int socketCount)
        {
            page = Encoding.UTF8.GetBytes(
                Encoding.UTF8.GetString(Properties.Resources.JSMain)
                .Replace("\"%{SOCKET_COUNT}%\"", socketCount.ToString()));
        }
        public void Write(HttpListenerResponse response)
        {
            response.ContentLength64 = page.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(page);
        }
    }
}
