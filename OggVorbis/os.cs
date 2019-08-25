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
    }
}
