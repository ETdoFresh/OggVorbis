﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OggVorbis.ArrayPointer;

namespace OggVorbis
{
    public class bitwise
    {
        public static readonly ulong[] mask =
{0x00000000,0x00000001,0x00000003,0x00000007,0x0000000f,
 0x0000001f,0x0000003f,0x0000007f,0x000000ff,0x000001ff,
 0x000003ff,0x000007ff,0x00000fff,0x00001fff,0x00003fff,
 0x00007fff,0x0000ffff,0x0001ffff,0x0003ffff,0x0007ffff,
 0x000fffff,0x001fffff,0x003fffff,0x007fffff,0x00ffffff,
 0x01ffffff,0x03ffffff,0x07ffffff,0x0fffffff,0x1fffffff,
 0x3fffffff,0x7fffffff,0xffffffff };

        public static void oggpack_readinit(oggpack_buffer b, ArrayPointer buf, long bytes)
        {
            //memset(buf, 0, sizeof(*b));
            b.buffer = b.ptr = buf;
            b.storage = bytes;
        }

        /* bits <= 32 */
        public static long oggpack_read(oggpack_buffer b, int bits)
        {
            long ret;
            ulong m;

            if (bits < 0 || bits > 32) goto err;
            m = mask[bits];
            bits += b.endbit;

            if (b.endbyte >= b.storage - 4)
            {
                /* not the main path */
                if (b.endbyte > b.storage - ((bits + 7) >> 3)) goto overflow;
                /* special case to avoid reading b.ptr[0], which might be past the end of
                    the buffer; also skips some useless accounting */
                else if (bits == 0) return (0L);
            }

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            ret = b.ptr[0] >> b.endbit;
            if (bits > 8)
            {
                ret |= b.ptr[1] << (8 - b.endbit);
                if (bits > 16)
                {
                    ret |= b.ptr[2] << (16 - b.endbit);
                    if (bits > 24)
                    {
                        ret |= b.ptr[3] << (24 - b.endbit);
                        if (bits > 32 && b.endbit != 0)
                        {
                            ret |= b.ptr[4] << (32 - b.endbit);
                        }
                    }
                }
            }
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            ret &= (long)m;
            b.ptr += bits / 8;
            b.endbyte += bits / 8;
            b.endbit = bits & 7;
            return ret;

            overflow:
            err:
            b.ptr = null;
            b.endbyte = b.storage;
            b.endbit = 1;
            return -1L;
        }

        public static long oggpack_bytes(oggpack_buffer b)
        {
            return (b.endbyte + (b.endbit + 7) / 8);
        }
    }
}
