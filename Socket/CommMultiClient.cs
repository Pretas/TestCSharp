using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SocketLib
{
    public class Server
    {
        public Socket ServerSocket { get; set; }
        List<Socket> ClientSockets { get; set; }
        int Port { get; set; }
        byte[] Buffer { get; set; }

        public void SetupServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 100);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(10);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        public void CloseAllSockets()
        {
            foreach (var sock in ClientSockets)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket sock = ServerSocket.EndAccept(ar);
            ClientSockets.Add(sock);
            sock.BeginReceive(Buffer, 0, 100, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            int recv = sock.EndReceive(ar);
            sock.BeginReceive(Buffer, 0, 100, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
    }

}