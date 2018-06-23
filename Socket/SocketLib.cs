using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SocketNetwork
{
    public class SocketNetworkingTools
    {
        public static void Send(Socket toSocket, object data)
        {
        }

        public static void Receive(Socket fromSocket)
        {
            byte[] dataLengthByte = new byte[1024];
            int a = fromSocket.Receive(dataLengthByte);
            int dataLength = BitConverter.ToInt32(dataLengthByte, 0);
            
            byte[] resp = BitConverter.GetBytes(true);
            fromSocket.Send(resp);

            byte[] data = new byte[dataLength];
            int a2 = fromSocket.Receive(data);
        }

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

    public class SerializationTools
    {
        public static byte[] SerializeOnMemory(object data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            ms.Position = 0;
            bf.Serialize(ms, data);
            byte[] res = ms.ToArray();
            ms.Dispose();

            return res;
        }

        public static object DeserializeOnMemory(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            ms.Position = 0;
            bf.Deserialize(ms);
            object res = ms.ToArray();
            ms.Dispose();

            return res;
        }        
   
    }
}
