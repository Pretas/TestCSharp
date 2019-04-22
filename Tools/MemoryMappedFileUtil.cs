using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class MemoryMappedFileUtil
    {
        public static MemoryMappedFile SaveMMF<T>(string name, bool isReuse, T obj)
        {
            byte[] byteObj = SerializationUtil.SerializeToByte(obj);

            bool isExists = false;

            try { var mmf = MemoryMappedFile.CreateNew(name, byteObj.Length); }
            catch { isExists = true; }

            if (!isReuse && isExists) throw new Exception(string.Format(@"MMF {0} is already exists", name));

            try
            {
                var mmf = MemoryMappedFile.CreateNew(name, byteObj.Length);

                using (var wt = mmf.CreateViewAccessor(0, byteObj.Length))
                {
                    int pos = 0;
                    wt.WriteArray(pos, byteObj, 0, byteObj.Length);
                }

                return mmf;
            }
            catch(Exception e)
            {
                throw e;
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

        public static T LoadMMF<T>(string name)
        {
            T res = default(T);

            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(name))
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

                return res;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw ex;
            }
        }
    }
}
