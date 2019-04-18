using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tools
{
    public static class SerializationUtil
    {
        public static byte[] SerializeToByte(object obj)
        {
            if (!CheckSerializable(obj))
            { return null; }
            MemoryStream stream = new MemoryStream();
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new AlwaysBrandNewDeserializationBinder(); 사용안하기로함
            bf.Serialize(stream, obj);
            return stream.ToArray();
        }

        public static object DeserializeToObject(byte[] byteData)
        {
            MemoryStream stream = new MemoryStream();
            stream.SetLength(byteData.Length + Int64.Parse(Math.Pow(2, 20).ToString()));
            foreach (byte b in byteData)
            {
                stream.WriteByte(b);
            }
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new AlwaysBrandNewDeserializationBinder(); 사용안하기로함
            return bf.Deserialize(stream);
        }

        public static bool CheckSerializable(object obj)
        {
            Type t = obj.GetType();
            var serializable = t.GetCustomAttributes(typeof(SerializableAttribute), false);
            if (!t.IsSerializable && serializable.Count() == 0)
            { return false; }
            else
            { return true; }
        }

        private class AlwaysBrandNewDeserializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                typeToDeserialize = Type.GetType(string.Format(@"{0}, {1}", typeName, assemblyName));
                return typeToDeserialize;
            }
        }
    }
}
