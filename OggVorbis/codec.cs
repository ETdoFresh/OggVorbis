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
using static OggVorbis.codec_internal;
using ogg_int64_t = System.Int64;

 namespace OggVorbis
{
    public class codec
    {
        public struct vorbis_info
        {
            public long version;
            public long channels;
            public long rate;

            /* The below bitrate declarations are *hints*.
               Combinations of the three values carry the following implications:

               all three set to the same value:
                 implies a fixed rate bitstream
               only nominal set:
                 implies a VBR stream that averages the nominal bitrate.  No hard
                 upper/lower limit
               upper and or lower set:
                 implies a VBR bitstream that obeys the bitrate limits. nominal
                 may also be set to give a nominal rate.
               none set:
                 the coder does not care to speculate.
            */

            public long bitrate_upper;
            public long bitrate_nominal;
            public long bitrate_lower;
            public long bitrate_window;

            public codec_setup_info codec_setup;
        }

        /* vorbis_dsp_state buffers the current vorbis audio
           analysis/synthesis state.  The DSP state belongs to a specific
           logical bitstream ****************************************************/
        public struct vorbis_dsp_state
        {
            public int analysisp;
            public vorbis_info vi;

            public float[][] pcm;
            public float[][] pcmret;
            public int pcm_storage;
            public int pcm_current;
            public int pcm_returned;

            public int preextrapolate;
            public int eofflag;

            public long lW;
            public long W;
            public long nW;
            public long centerW;

            public ogg_int64_t granulepos;
            public ogg_int64_t sequence;

            public ogg_int64_t glue_bits;
            public ogg_int64_t time_bits;
            public ogg_int64_t floor_bits;
            public ogg_int64_t res_bits;

            public object backend_state;
        }

        public struct vorbis_block
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
            public vorbis_dsp_state vd; /* For read-only access of configuration */

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

        /* vorbis_block is a single block of data to be processed as part of
        the analysis/synthesis stream; it belongs to a specific logical
        bitstream, but is independent from other vorbis_blocks belonging to
        that logical bitstream. *************************************************/

        public class alloc_chain
        {
            public object ptr;
            public alloc_chain next;
        }

        /* vorbis_info contains all the setup information specific to the
           specific compression/decompression mode in progress (eg,
           psychoacoustic settings, channel setup, options, codebook
           etc). vorbis_info and substructures are in backends.h.
        *********************************************************************/

        /* the comments are not part of vorbis_info so that vorbis_info can be
           static storage */
        public struct vorbis_comment
        {
            /* unlimited user comment fields.  libvorbis writes 'libvorbis'
               whatever vendor is set to in encode */
            public byte[][] user_comments;
            public long[] comment_lengths;
            public long comments;
            public byte[] vendor;

        }

        /* libvorbis encodes in two abstraction layers; first we perform DSP
           and produce a packet (see docs/analysis.txt).  The packet is then
           coded into a framed OggSquish bitstream by the second layer (see
           docs/framing.txt).  Decode is the reverse process; we sync/frame
           the bitstream and extract individual packets, then decode the
           packet back into PCM audio.

           The extra framing/packetizing is used in streaming formats, such as
           files.  Over the net (such as with UDP), the framing and
           packetization aren't necessary as they're provided by the transport
           and the streaming layer is not used */

        /* Vorbis ERRORS and return codes ***********************************/
        public const int OV_FALSE = -1;
        public const int OV_EOF = -2;
        public const int OV_HOLE = -3;

        public const int OV_EREAD = -128;
        public const int OV_EFAULT = -129;
        public const int OV_EIMPL = -130;
        public const int OV_EINVAL = -131;
        public const int OV_ENOTVORBIS = -132;
        public const int OV_EBADHEADER = -133;
        public const int OV_EVERSION = -134;
        public const int OV_ENOTAUDIO = -135;
        public const int OV_EBADPACKET = -136;
        public const int OV_EBADLINK = -137;
        public const int OV_ENOSEEK = -138;
    }
}