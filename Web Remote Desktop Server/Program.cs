// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using WebRemoteDesktopServer;

[assembly: Guid("5DC6642C-6C5B-4445-8ED7-47E5EE77FCD9")]
var worker = new Worker();
worker.Execute();
