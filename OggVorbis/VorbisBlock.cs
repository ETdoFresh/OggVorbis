/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2001             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *

 ********************************************************************

 function: libvorbis codec headers
  
 Ported by: ETdoFresh

 ********************************************************************/
using ogg_int64_t = System.Int64;

namespace OggVorbis
{
    public struct VorbisBlock
    {
        /* necessary stream state for linking to the framing abstraction */
        public float[][] pcm;       /* this is a pointer into local storage */
        public oggpack_buffer opb;

        public long lW;
        public long W;
        public long nW;
        public int pcmend;
        public int mode;

        public int eofflag;
        public ogg_int64_t granulepos;
        public ogg_int64_t sequence;
        public VorbisDspState vd; /* For read-only access of configuration */

        /* local storage to avoid remallocing; it's up to the mapping to
           structure it */
        public object localstore;
        public long localtop;
        public long localalloc;
        public long totaluse;
        public alloc_chain[] reap;

        /* bitmetrics for the frame */
        public long glue_bits;
        public long time_bits;
        public long floor_bits;
        public long res_bits;

        public object @internal;
    }
}