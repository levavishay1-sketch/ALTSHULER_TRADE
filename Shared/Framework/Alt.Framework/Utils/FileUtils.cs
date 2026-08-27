using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alt.Framework.Utils
{
    public static class FileUtils
    {
        public static int GetBase64FileSizeInBytes(string base64string)
        {
            if (string.IsNullOrEmpty(base64string)) { return 0; }

            var characterCount = base64string.Length;
            var paddingCount = base64string.Substring(characterCount - 2, 2)
                                           .Count(c => c == '=');
            return (3 * (characterCount / 4)) - paddingCount;
        }
 
        public static byte[] ConvertStreamToByteArrayUsingBinaryReaderReader(Stream stream)
        {
            byte[] bytes;
            using (var binaryReader = new BinaryReader(stream, Encoding.GetEncoding(1255)))
            {
                bytes = binaryReader.ReadBytes((int)stream.Length);
            }
            return bytes;
        }
        public static byte[] ConvertStreamToByteArrayUsingStreamReader(Stream stream)
        {
            byte[] bytes;
            using (var reader = new StreamReader(stream))
            {
                bytes = System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd());
            }
            return bytes;
        }

        public static byte[] ConvertStreamToByteArrayUsingMemoryStream(Stream stream)
        {
            byte[] bytes;
            if (stream is MemoryStream memorystream)
            {
                bytes = memorystream.ToArray();
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }

            return bytes;
        }

        public static byte[] ConvertStreamToByteArrayUsingBufferedStream(Stream stream)
        {
            byte[] bytes;
            using (var bufferedStream = new BufferedStream(stream))
            {
                using (var memoryStream = new MemoryStream())
                {
                    bufferedStream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }
            return bytes;
        }

        public static string ConvertStreamToBase64(Stream stream)
        {
            byte[] bytes = ConvertStreamToByteArrayUsingBinaryReaderReader(stream);
            return Convert.ToBase64String(bytes);
        }

        public static string ConvertStreamToBase64<T>(Stream stream) where T : Stream
        {
            string result = null;
            Dictionary<Type, Delegate> delegatesByStreamType = new Dictionary<Type, Delegate>
            {
                {typeof(BufferedStream),new Func<Stream,byte[]>(ConvertStreamToByteArrayUsingBufferedStream)},
                {typeof(MemoryStream),new Func<Stream,byte[]>(ConvertStreamToByteArrayUsingMemoryStream)},
                {typeof(StreamReader),new Func<Stream,byte[]>(ConvertStreamToByteArrayUsingStreamReader)},

            };
            Type type = typeof(T);

            if (delegatesByStreamType.ContainsKey(type))
            {
                byte[] bytes = (byte[])delegatesByStreamType[type].DynamicInvoke(stream);
                result = Convert.ToBase64String(bytes);
            }
            return result;
        }

        public static Stream ReadFileAsStream(string filePath)
        {
            Stream stream = null;
            if (File.Exists(filePath))
            {
                stream = File.OpenRead(filePath);
            }
            return stream;
        }
    }
}
