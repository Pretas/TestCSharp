using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace ConsoleTest
{    
    class Program
    {
        static void Main(string[] args)
        {
            MultiClientsTest();

            //Test();

            Console.ReadLine();
        }

        private static void Test()
        {
            int row = 100000;
            int col = 50;

            double[,] sample = new double[row, col];
            Random rd = new Random();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sample[i, j] = rd.NextDouble();
                }
            }

            string path = @"D:\StreamTest.txt";

            //파일저장
            File.Delete(path);
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, sample);
            }

            //파일열기
            double[,] res= new double[1,1];
            byte[] res2;
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                res2 = (byte[])bformatter.Deserialize(stream);
                //res = (double[,])bformatter.Deserialize(stream);                
            }

            long sum = 0;
            for (int i = 0; i < res2.GetLength(0); i++)
            {
                sum += res2[i];
            }
        }

        private static void MultiClientsTest()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            
            string role = Console.ReadLine();

            int loopNo = 10000;

            if (role == @"s")
            {
                int clCnt = 1;

                SocketLib.ServerTest sv = new SocketLib.ServerTest();
                sv.SetupServer(100, clCnt);

                List<string> recvM = new List<string>();                

                Action<object> ac = x =>
                {
                    Socket sock = (Socket)x;

                    for (int i = 0; i < loopNo; i++)
                    {
                        string recv = SocketLib.SendReceive.Receive<string>(sock);
                        recvM.Add(recv);
                    }                       
                };

                Task[] tasks = new Task[clCnt];

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i] = new Task(ac, sv.ClientSockets[i]);
                    tasks[i].Start();
                }

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i].Wait();
                }
                
                Console.WriteLine("server finished");
                Console.ReadKey();
            }
            else
            {
                SocketLib.Client cl = new SocketLib.Client(addr[1].ToString(), 100);
                cl.ConnectToServer();
                
                for (int i = 0; i < loopNo; i++)
                {
                    string msg = string.Format(@"{0}_{1}", role, i);
                    SocketLib.SendReceive.Send<string>(cl.ClientSocket, msg);
                }

                cl.Exit();

                Console.WriteLine("client finished");
                Console.ReadKey();
            }

        }
    }
}
