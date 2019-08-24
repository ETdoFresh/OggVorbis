/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2002             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: Define a consistent set of types on each platform.

 ********************************************************************/
using System;
using ogg_int16_t = System.Int16;
using ogg_uint16_t = System.UInt16;
using ogg_int32_t = System.Int32;
using ogg_uint32_t = System.UInt32;
using ogg_int64_t = System.Int64;
using ogg_uint64_t = System.UInt64;
using System.IO;
using System.Text;

namespace OggVorbis
{
    public class os_types
    {
        /* make it easy on the folks that want to compile the libs with a
           different malloc than stdlib */
        public static byte[] _ogg_malloc(long size)
        {
            return new byte[size];
        }

        public static byte[] _ogg_calloc(long size)
        {
            return new byte[size];
        }

        public static T[] _ogg_realloc<T>(T[] t, long size)
        {
            var realloc = new T[size];
            Array.Copy(t, realloc, t.Length);
            return realloc;
        }

        public static void memmove<T>(T[] array, long destination, long source, long length)
        {
            Array.Copy(array, source, array, destination, length);
        }

        public static void memset<T>(T[] array, long offset, T value, long length)
        {
            for (long i = offset; i < offset + length; i++)
                array[i] = value;
        }

        public static void memset<T>(T[] array, T value, long length) =>
            memset(array, 0, value, length);

        public static int fread(byte[] array, long size, long count, Stream stream)
        {
            return stream.Read(array, 0, (int)(count * size));
        }

        public static int memcmp<T>(T[] array1, T[] array2, long length) =>
            memcmp(array1, array2, 0, length);

        public static int memcmp<T>(T[] array1, T[] array2, long offset, long length)
        {
            for (long i = offset; i < offset + length; i++)
                if (!array1[i - offset].Equals(array2[i]))
                    return 1;
            return 0;
        }

        public static int memcmp(byte[] bytes, string s, long length)
        {
            var sBytes = Encoding.UTF8.GetBytes(s);
            return memcmp(bytes, sBytes, length);
        }

        public static int memcpy<T>(T[] array1, T[] array2, long length) =>
            memcpy(array1, array2, 0, length);

        public static int memcpy<T>(T[] array1, T[] array2, long offset, long length)
        {
            for (long i = offset; i < offset + length; i++)
                array1[i - offset] = array2[i];
            return (int)length;
        }

        public static int memcpy<T>(T[] array1, long offset, T[] array2, long length)
        {
            for (long i = offset; i < offset + length; i++)
                array1[i] = array2[i - offset];
            return (int)length;
        }

        public static int memchr(byte[] bytes, long offset, char c, long length)
        {
            for (long i = offset; i < offset + length; i++)
                if (bytes[i].Equals(Convert.ToByte(c)))
                    return (int)(i - offset);
            return -1;
        }

        public static void fprintf(Stream stream, string format, params object[] args)
        {
            var str = string.Format(format, args);
            var bytes = Encoding.UTF8.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void exit(int i)
        {
            Environment.Exit(i);
        }
    }
}