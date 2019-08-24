using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public static class OS
    {
        public static void memmove(MemoryStream stream, int data, int v, int fill)
        {
            var bytes = new byte[fill];
            stream.Position = v;
            stream.Read(bytes, 0, fill);
            stream.Position = 0;
            stream.Write(bytes, 0, fill);
            stream.SetLength(fill);
        }

        public static int fread(ref ogg_sync_state oggSyncState, int size, int count, Stream stream)
        {
            int bytes = 0;
            for (int i = 0; i < size * count; i++)
            {
                var bite = stream.ReadByte();
                if (bite != -1)
                {
                    oggSyncState.stream.WriteByte((byte)bite);
                    bytes++;
                }
                else
                    break;
            }
            return bytes;
        }

        public static int memcmp(MemoryStream stream, int streamPosition, string value, int size)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return memcmp(stream, valueBytes, streamPosition, size);
        }

        public static byte ReadAt(this MemoryStream stream, long streamPosition)
        {
            var position = stream.Position;
            stream.Position = streamPosition;
            var bite = stream.ReadByte();
            if (bite == -1)
                throw new Exception("End of stream, cannot read");
            stream.Position = position;
            return (byte)bite;
        }

        public static void memcpy(this MemoryStream stream, byte[] bytes, long streamPosition, int size)
        {
            var position = stream.Position;
            stream.Position = streamPosition;
            for (int i = 0; i < size; i++)
                bytes[i] = (byte)stream.ReadByte();
            stream.Position = position;
        }

        public static void memset(this MemoryStream stream, long position, byte value, int size)
        {
            var originalPosition = stream.Position;
            stream.Position = position;
            for (int i = 0; i < size; i++)
                stream.WriteByte(value);
            stream.Position = originalPosition;
        }

        public static void WriteAt(this MemoryStream stream, long position, byte value)
        {
            var originalPosition = stream.Position;
            stream.Position = position;
            stream.WriteByte(value);
            stream.Position = originalPosition;
        }

        public static int memcmp(this MemoryStream stream, byte[] bytes, long streamPosition, int size)
        {
            var position = stream.Position;
            stream.Position = streamPosition;
            for (int i = 0; i < size; i++)
            {
                var streamByte = (byte)stream.ReadByte();
                if (bytes[i] != streamByte)
                {
                    stream.Position = position;
                    return 1;
                }
            }

            stream.Position = position;
            return 0;
        }

        public static void memcpy(this MemoryStream stream, long streamPosition, byte[] bytes, int size)
        {
            var position = stream.Position;
            stream.Position = streamPosition;
            for (int i = 0; i < size; i++)
                stream.WriteByte(bytes[i]);
            stream.Position = position;
        }

        public static int memchr(this MemoryStream stream, long streamPosition, byte value, int size)
        {
            var position = stream.Position;
            stream.Position = streamPosition;
            for (int i = 0; i < size; i++)
                if (stream.ReadByte() == value)
                {
                    stream.Position = position;
                    return i;
                }
            stream.Position = position;
            return 0;
        }

        public static void fprintf(Stream stream, string format, params object[] args)
        {
            var str = string.Format(format, args);
            var bytes = Encoding.UTF8.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void exit(int exitCode)
        {
            System.Environment.Exit(exitCode);
        }
    }
}
