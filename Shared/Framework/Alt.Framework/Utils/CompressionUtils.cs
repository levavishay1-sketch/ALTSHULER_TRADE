using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alt.Framework.Utils
{
    public enum CompressionType
    {
        GZip
    }

    public static class CompressionUtils
    {
        public static byte[] ToCompressedJson(byte[] batch, CompressionType type)
        {
            string json = JsonSerializer.Serialize(batch);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var outStream = new MemoryStream())
            {
                if (type == CompressionType.GZip)
                {
                    using (var compressor = new GZipStream(outStream, CompressionLevel.Fastest, leaveOpen: true))
                    {
                        compressor.Write(jsonBytes, 0, jsonBytes.Length);
                    }
                }
                else
                {
                    throw new NotSupportedException("Unsupported compression type.");
                }

                return outStream.ToArray();
            }
        }

        public static byte[] FromCompressedJson(byte[] compressed, CompressionType type)
        {
            using (var inStream = new MemoryStream(compressed))
            {
                Stream decompressor = null;

                if (type == CompressionType.GZip)
                {
                    decompressor = new GZipStream(inStream, CompressionMode.Decompress);
                }
                else
                {
                    throw new NotSupportedException("Unsupported decompression type.");
                }

                using (decompressor)
                {
                    using (var reader = new StreamReader(decompressor))
                    {
                        string json = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<byte[]>(json);
                    }
                }
            }
        }
    }
}
