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

        public static void memmove(int[] array, long sourceIndex, long size)
        {
            var temp = new int[size];
            Array.Copy(array, sourceIndex, temp, 0, size);
            Array.Copy(temp, array, size);
        }

        public static void memmove(long[] array, long sourceIndex, long size)
        {
            var temp = new int[size];
            Array.Copy(array, sourceIndex, temp, 0, size);
            Array.Copy(temp, array, size);
        }

        public static T[] _ogg_realloc<T>(T[] array, long newSize)
        {
            Array.Resize(ref array, (int)newSize);
            return array;
        }
    }
}
