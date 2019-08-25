using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public class codec
    {
        /* Vorbis ERRORS and return codes ***********************************/

        public static int OV_FALSE = -1;
        public static int OV_EOF = -2;
        public static int OV_HOLE = -3;

        public static int OV_EREAD = -128;
        public static int OV_EFAULT = -129;
        public static int OV_EIMPL = -130;
        public static int OV_EINVAL = -131;
        public static int OV_ENOTVORBIS = -132;
        public static int OV_EBADHEADER = -133;
        public static int OV_EVERSION = -134;
        public static int OV_ENOTAUDIO = -135;
        public static int OV_EBADPACKET = -136;
        public static int OV_EBADLINK = -137;
        public static int OV_ENOSEEK = -138;
    }
}
