using System.Net;

namespace WebRemoteDesktopServer.Web.Page
{
    class WebJavascriptAudioProcessorPage : IPage
    {
        public void Write(HttpListenerResponse response)
        {
            var value = Properties.Resources.JSAudioProcessor;
            response.ContentLength64 = value.LongLength;
            response.ContentType = "text/javascript";
            response.OutputStream.Write(value);
        }
    }
}
