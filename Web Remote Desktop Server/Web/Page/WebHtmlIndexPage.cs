using System.Net;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebHtmlIndexPage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Properties.Resources.HTMLIndex;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/html";
            response.OutputStream.Write(value);
        }
    }
}
