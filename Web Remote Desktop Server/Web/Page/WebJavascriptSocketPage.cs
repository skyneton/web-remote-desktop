using System.Net;
using WebRemoteDesktopServer.Properties;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptSocketPage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Resources.JSSocket;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(value);
        }
    }
}
