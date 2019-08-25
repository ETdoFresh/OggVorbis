/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2007             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: toplevel libogg include

 ********************************************************************/
using ogg_int64_t = System.Int64;

namespace OggVorbis
{
    /* ogg_packet is used to encapsulate the data and metadata belonging
       to a single raw Ogg/Vorbis packet *************************************/

    public class ogg_packet
    {
        public byte[] packet;
        public long bytes;
        public long b_o_s;
        public long e_o_s;

        public ogg_int64_t granulepos;

        public ogg_int64_t packetno; /* sequence number for decode; the framing
                                    knows where there's a hole in the data,
                                    but we need coupling so that the codec
                                    (which is in a separate abstraction
                                    layer) also knows about the gap */
    }
}