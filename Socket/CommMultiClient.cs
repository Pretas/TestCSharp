using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace SocketLib
{
    public class ServerAsync
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
    
    public class Server
    {
        public Socket ServerSocket { get; set; }
        public List<Socket> ClientSockets { get; set; }
        int Port { get; set; }
        int ClientCnt { get; set; }
        byte[] Buffer { get; set; }

        public void SetupServer(int port, int clientCnt)
        {
            this.Port = port;
            this.ClientCnt = clientCnt;

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            IPEndPoint ep = new IPEndPoint(addr[1], 100);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(10);

            ClientSockets = new List<Socket>();

            for (int i = 0; i < ClientCnt; i++)
            {
                Socket sock = ServerSocket.Accept();
                ClientSockets.Add(sock);
            }

            Console.WriteLine("Server Connection Finished");
        }

        public void CloseAllSockets()
        {
            foreach (var sock in ClientSockets)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
        }
    }

    public class Client
    {
        public IPAddress ServerIP { get; private set; }
        public int Port { get; private set; }
        public Socket ClientSocket { get; private set; }

        public Client(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            Port = port;
        }

        public void ConnectToServer()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            int tryCnt = 0;

            while (!ClientSocket.Connected)
            {
                tryCnt++;

                Thread.Sleep(1000);
                IPEndPoint ep = new IPEndPoint(ServerIP, Port);
                ClientSocket.Connect(ep);
                
                if (tryCnt == 20) throw new Exception(string.Format("faild to connect to server {0}", ServerIP.ToString()));
            }

            Console.WriteLine("connection");
        }

        public void Exit()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }
    }    
}