using System.Net;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptByteBufPage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Properties.Resources.JSByteBuf;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(value);
        }
    }
}
