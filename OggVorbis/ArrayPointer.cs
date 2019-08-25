using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public class ArrayPointer
    {
        private Stream stream;
        public long pointer = 0;

        public ArrayPointer()
        {
            stream = new MemoryStream();
        }

        public ArrayPointer(Stream stream)
        {
            this.stream = stream;
        }

        public byte this[long i]
        {
            get => ReadAt(pointer + i);
            set => WriteAt(pointer + i, value);
        }

        public byte Read()
        {
            var value = ReadAt(pointer);
            pointer++;
            return value;
        }

        public byte[] Read(long size)
        {
            var value = ReadAt(pointer, size);
            pointer += size;
            return value;
        }

        public void Write(byte value)
        {
            WriteAt(pointer, value);
            pointer++;
        }

        public void Write(byte[] bytes)
        {
            WriteAt(pointer, bytes);
            pointer += bytes.Length;
        }

        public byte ReadAt(long i)
        {
            var position = stream.Position;
            stream.Position = i;
            var value = stream.ReadByte();
            stream.Position = position;
            if (value == -1)
                throw new Exception("End of Stream");
            return (byte)value;
        }

        public void WriteAt(long i, byte value)
        {
            var position = stream.Position;
            stream.Position = i;
            stream.WriteByte(value);
            stream.Position = position;
        }

        public byte[] ReadAt(long i, long size)
        {
            var bytes = new byte[size];
            var position = stream.Position;
            stream.Position = i;
            for (var j = 0; j < size; j++)
            {
                var value = stream.ReadByte();
                if (value == -1)
                    throw new Exception("End of Stream");
                bytes[j] = (byte)value;
            }
            stream.Position = position;
            return bytes;
        }

        public byte[] WriteAt(long i, byte[] bytes)
        {
            var position = stream.Position;
            stream.Position = i;
            for (var j = 0; j < bytes.LongLength; j++)
                stream.WriteByte(bytes[j]);
            stream.Position = position;
            return bytes;
        }

        public void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public static ArrayPointer operator +(ArrayPointer a, ArrayPointer b)
        {
            if (a.stream != b.stream)
                throw new Exception("Not the same streams!");

            return new ArrayPointer { stream = a.stream, pointer = a.pointer + b.pointer };
        }

        public static ArrayPointer operator +(ArrayPointer a, long b)
        {
            return new ArrayPointer { stream = a.stream, pointer = a.pointer + b };
        }

        public static implicit operator long(ArrayPointer a)
        {
            return a.pointer;
        }

        public static void memmove(ArrayPointer destination, long source, long size)
        {
            var bytes = destination.Read(size);
            destination.WriteAt(source, bytes);
            destination.SetLength(source + size);
        }

        public static int fread(ArrayPointer buffer, long size, long count, Stream stream)
        {
            int bytes = 0;
            for (int i = 0; i < size * count; i++)
            {
                var bite = stream.ReadByte();
                if (bite != -1)
                {
                    buffer.Write((byte)bite);
                    bytes++;
                }
                else
                    break;
            }
            return bytes;
        }

        public static int memcmp(ArrayPointer a, string value, long size)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return memcmp(a, valueBytes, size);
        }

        public static int memcmp(byte[] bytes, ArrayPointer a, long size)
            => memcmp(a, bytes, size);

        public static int memcmp(ArrayPointer a, byte[] bytes, long size)
        {
            var pointer = a.pointer;
            for (int i = 0; i < size; i++)
            {
                if (bytes[i] != a.Read())
                {
                    a.pointer = pointer;
                    return 1;
                }
            }
            a.pointer = pointer;
            return 0;
        }

        public static void memcpy(byte[] destination, ArrayPointer source, long size)
        {
            var pointer = source.pointer;
            for (int i = 0; i < size; i++)
                destination[i] = source.Read();
            source.pointer = pointer;
        }

        public static void memcpy(ArrayPointer destination, byte[] source, long size)
        {
            var pointer = destination.pointer;
            for (int i = 0; i < size; i++)
                destination.Write(source[i]);
            destination.pointer = pointer;
        }

        public static void memcpy(ArrayPointer destination, ArrayPointer source, long size)
        {
            var destinationPointer = destination.pointer;
            var sourcePointer = source.pointer;
            for (int i = 0; i < size; i++)
                destination.Write(source.Read());
            destination.pointer = destinationPointer;
            source.pointer = sourcePointer;
        }

        public static void memset(ArrayPointer destination, byte value, long size)
        {
            var pointer = destination.pointer;
            for (int i = 0; i < size; i++)
                destination.Write(value);
            destination.pointer = pointer;
        }

        public static ArrayPointer memchr(ArrayPointer array, char value, long size)
            => memchr(array, Convert.ToByte(value), size);

        public static ArrayPointer memchr(ArrayPointer array, byte value, long size)
        {
            var pointer = array.pointer;
            for (int i = 0; i < size; i++)
                if (array.Read() == value)
                {
                    array.pointer = pointer;
                    return array + i;
                }
            array.pointer = pointer;
            return null;
        }
    }
}
