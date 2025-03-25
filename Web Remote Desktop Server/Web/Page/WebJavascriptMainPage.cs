using System.Net;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptMainPage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Properties.Resources.JSMain;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(value);
        }
    }
}
