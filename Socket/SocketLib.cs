using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Tools;
using System.Threading;

namespace SocketTools
{
    public class SocketNetworkingTools
    {
        public static void SendOnFileStreamOld(Socket toSocket, string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            int fileLength = (int)stream.Length;
            byte[] buffer = BitConverter.GetBytes(fileLength);
            toSocket.Send(buffer);

            byte[] resp = new byte[1024];
            toSocket.Receive(resp);

            BinaryReader br = new BinaryReader(stream);
            byte[] data = br.ReadBytes(fileLength);
            toSocket.Send(data);
        }

        public static void ReceiveOnFileStreamOld(Socket fromSocket, string fileName)
        {
            byte[] fileLength = new byte[1024];
            fromSocket.Receive(fileLength);
            int dataLength = BitConverter.ToInt32(fileLength, 0);

            byte[] resp = BitConverter.GetBytes(true);
            fromSocket.Send(resp);
            
            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(stream);
            byte[] data = new byte[dataLength];
            fromSocket.Receive(data);
            w.Write(data, 0, dataLength);
        }

        public static void SendOnFileStream(Socket toSocket, string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            int fileLength = (int)stream.Length;
            byte[] buffer = BitConverter.GetBytes(fileLength);
            toSocket.Send(buffer);

            byte[] resp = new byte[1024];
            toSocket.Receive(resp);

            BinaryReader br = new BinaryReader(stream);
            byte[] data = br.ReadBytes(fileLength);
            toSocket.Send(data);
        }

        public static void ReceiveOnFileStream(Socket fromSocket, string fileName)
        {
            byte[] fileLength = new byte[1024];
            fromSocket.Receive(fileLength);
            int dataLength = BitConverter.ToInt32(fileLength, 0);

            byte[] resp = BitConverter.GetBytes(true);
            fromSocket.Send(resp);

            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(stream);
            byte[] data = new byte[dataLength];
            fromSocket.Receive(data);
            w.Write(data, 0, dataLength);
        }
    }

    public class SendReceive
    {
        public static void Send<T>(Socket clientSock, T obj)
        {
            byte[] dataByte = SerializationUtil.SerializeToByte(obj);
            send(clientSock, dataByte);
        }

        public static void SendByBuffer<T>(Socket clientSock, int byteCntBySending, T obj)
        {
            byte[] dataByte = SerializationUtil.SerializeToByte(obj);

            send(clientSock, SerializationUtil.SerializeToByte(dataByte.Length));

            int unit = byteCntBySending; // 1mb씩 보내기

            int bunchCnt = dataByte.Length/unit + (dataByte.Length % unit > 1 ? 1 : 0);
                        
            send(clientSock, SerializationUtil.SerializeToByte(bunchCnt));

            int now = 0;

            for (int i = 0; i < bunchCnt; i++)
            {
                int byteCnt = Math.Min(byteCntBySending, dataByte.Length - now);

                byte[] sub = new byte[byteCnt];
                Array.Copy(dataByte, now, sub, 0, byteCnt);
                now += byteCnt;

                //send(clientSock, SerializationUtil.SerializeToByte(byteCnt));
                send(clientSock, sub);
                //clientSock.Send(sub);
            }
        }

        protected static void send(Socket clientSock, byte[] data)
        {
            // 객체의 바이트수 계산, null이거나 바이트가 0이면 실데이터는 전송하지 않음
            int dl = 0;
            if (data != null || data.Length == 0) dl = data.Length;

            // 객체의 바이트수 전송
            byte[] dlb = BitConverter.GetBytes(dl);
            clientSock.Send(dlb);

            // 객체의 바이트수 답변 받음
            byte[] lb1 = GetBytesFromStream(clientSock, 4);

            // 객체의 바이트수가 잘 전달되었는지 체크
            bool isRightLength = true;
            for (int i = 0; i < 4; i++)
            {
                if (dlb[i] != lb1[i]) isRightLength = false;
            }

            // 잘 전달되었는지 아닌지 전송, 잘못 전송되었다면 예외처리
            if (isRightLength == true)
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"!#%&("));
            }
            else
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"@$^*)"));
                throw new Exception(@"incorrect message length sended");
            }

            // 바이트수가 0 이상이어야 실데이터가 있으므로 전송
            if (dl > 0)
            {
                //메모리전송
                clientSock.Send(data);
            }
        }

        public static T Receive<T>(Socket clientSock)
        {
            byte[] dataByte = Receive(clientSock);
            
            if (dataByte != null)
            {                
                return (T)SerializationUtil.DeserializeToObject(dataByte);
            }
            else
            {
                throw new Exception("null data was transferred.");
            }
        }

        public static T ReceiveByBuffer<T>(Socket clientSock)
        {
            int byteCnt = Receive<int>(clientSock);
            int bunchCnt = Receive<int>(clientSock);

            List<byte> total = new List<byte>();

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = Receive(clientSock);
                total.AddRange(received.ToList());
            }

            return (T)SerializationUtil.DeserializeToObject(total.ToArray());
        }

        public static T ReceiveByBufferOnMMF<T>(Socket clientSock)
        {
            int byteCnt = Receive<int>(clientSock);
            int bunchCnt = Receive<int>(clientSock);

            List<byte> total = new List<byte>();

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = Receive(clientSock);
                total.AddRange(received.ToList());
            }

            return (T)SerializationUtil.DeserializeToObject(total.ToArray());
        }

        protected static byte[] Receive(Socket clientSock)
        {
            // 객체의 바이트수 수신
            byte[] dlb = GetBytesFromStream(clientSock, 4);

            // 객체 바이트수 발신(echo)
            clientSock.Send(dlb);

            // 올바른 바이트수였는지 여부 수신
            // : "!#%&(" 이면 OK, "@$^*)"이면 에러
            byte[] respond = GetBytesFromStream(clientSock, 5);

            // 데이터 길이가 맞는지 다시 확인받음
            string respondStr = Encoding.UTF8.GetString(respond);
            if (respondStr == @"@$^*)") throw new Exception(@"incorrect message length received");

            int lth = BitConverter.ToInt32(dlb, 0);
            if (lth > 0)
            {
                byte[] dataReceived = GetBytesFromStream(clientSock, lth);
                return dataReceived;
            }
            else return null;
        }

        private static byte[] GetBytesFromStream(Socket sock, int lth)
        {
            byte[] dataBytes = new byte[lth];
            int countReceived = sock.Receive(dataBytes);
            if (countReceived < lth)
            {
                int lthNested = lth - countReceived;
                byte[] dataBytesNested = GetBytesFromStream(sock, lthNested);
                for (int i = 0; i < lthNested; i++) dataBytes[countReceived + i] = dataBytesNested[i];
            }

            return dataBytes;
        }
    }

    public class ServerSocket
    {
        public Socket sock;
        public Socket clientSock;

        public ServerSocket(int port)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 포트에 바인드
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            sock.Bind(ep);

            // (3) 포트 Listening 시작
            sock.Listen(10);

            // (4) 연결을 받아들여 새 소켓 생성 (하나의 연결만 받아들임)
            clientSock = sock.Accept();
        }

        public void Close()
        {
            // (7) 소켓 닫기
            clientSock.Close();
            sock.Close();
        }
    }

    public class ClientSocket
    {
        public Socket sock;

        public ClientSocket(string serverIP, int port)
        {
            // (1) 소켓 객체 생성 (TCP 소켓)
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            int tryCount = 20;
            int tryCounter = 0;
            while (tryCounter < tryCount)
            {
                try
                {
                    // (2) 서버에 연결
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIP), port);
                    sock.Connect(ep);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(@"Client for {0}/{1} : {2}", serverIP, port.ToString(), e.Message);
                    Thread.Sleep(10000);
                }
                finally
                {
                    tryCounter++;
                }
            }
        }

        public void Close()
        {
            // (5) 소켓 닫기
            sock.Close();
        }
    }
}
