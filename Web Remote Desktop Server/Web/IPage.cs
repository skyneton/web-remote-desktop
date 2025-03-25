using System.Net;

namespace WebRemoteDesktopServer.Web
{
    interface IPage
    {
        void Write(HttpListenerResponse response);
    }
}
