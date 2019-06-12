using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    public class MemoryMappedFileUtil
    {
        public static MemoryMappedFile SaveMMF<T>(T obj, string fileName, bool isReuse)
        {
            MemoryMappedFile mmf;

            bool isExists = false;

            try
            {
                using (var item = MemoryMappedFile.OpenExisting(fileName)) { }

                isExists = true;

                if (isReuse) mmf = MemoryMappedFile.OpenExisting(fileName);
                else throw new Exception("There is file " + fileName + "already");

                return mmf;
            }
            catch(Exception e)
            {
                if (!isReuse && isExists) throw e;
                
                try
                {
                    byte[] byteObj = SerializationUtil.SerializeToByte(obj);

                    mmf = MemoryMappedFile.CreateNew(fileName, byteObj.Length);

                    int lth = byteObj.Length;

                    using (var wt = mmf.CreateViewAccessor(0, lth))
                    {
                        int pos = 0;
                        wt.WriteArray(pos, byteObj, 0, lth);
                    }

                    return mmf;
                }
                catch (Exception e2)
                {
                    throw e2;
                }
            }
        }

        public static MemoryMappedFile SaveMMF(string name, Socket sock)
        {
            long byteCnt = SendReceive.Receive<long>(sock);
            int bunchCnt = SendReceive.Receive<int>(sock);

            // using 쓰면 안됨, isClosed = true로 바뀜
            var mmf = MemoryMappedFile.CreateNew(name, byteCnt);

            long position = 0;

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = SendReceive.Receive(sock);

                using (var wt = mmf.CreateViewAccessor(position, received.Length))
                {
                    int pos = 0;
                    wt.WriteArray(pos, received, 0, received.Length);
                    position += received.Length;
                }
            }

            return mmf;            
        }

        public static T LoadMMF<T>(string fileName)
        {
            T res = default(T);

            Thread t = new Thread(() =>
            {
                try
                {
                    using (var mmf = MemoryMappedFile.OpenExisting(fileName))
                    {
                        using (var stream = mmf.CreateViewStream())
                        {
                            using (BinaryReader br = new BinaryReader(stream))
                            {
                                byte[] rt = br.ReadBytes((int)stream.Length);
                                res = (T)SerializationUtil.DeserializeToObject(rt);
                            }
                        }
                    }
                }
                catch
                {
                    throw new Exception(string.Format("mmf {0} does not exists.", fileName));
                }
            });

            t.Start();

            if (!t.Join(TimeSpan.FromSeconds(30)))
            {
                t.Abort();
                throw new Exception(string.Format("Deadlock occured on mmf {0}", fileName));
            }

            return res;
        }
    }
}
