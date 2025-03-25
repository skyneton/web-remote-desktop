using System.Net;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebCssStylePage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Properties.Resources.CssStyle;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/css";
            response.OutputStream.Write(value);
        }
    }
}
