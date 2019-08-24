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
    /* ogg_stream_state contains the current encode/decode state of a logical
       Ogg bitstream **********************************************************/

    public class OggStreamState
    {
        public byte[] body_data;           /* bytes from packet bodies */
        public long body_storage;          /* storage elements allocated */
        public long body_fill;             /* elements stored; fill mark */
        public long body_returned;         /* elements of fill returned */


        public int[] lacing_vals;          /* The values that will go to the segment table */
        public ogg_int64_t[] granule_vals; /* granulepos values for headers. Not compact
                                         this way, but it is simple coupled to the
                                         lacing fifo */
        public long lacing_storage;
        public long lacing_fill;
        public long lacing_packet;
        public long lacing_returned;

        public byte[] header/*= new byte[282]*/;      /* working space for header encode */
        public int header_fill;

        public int e_o_s;          /* set when we have buffered the last packet in the
                             logical bitstream */
        public int b_o_s;          /* set after we've written the initial page
                             of a logical bitstream */
        public long serialno;
        public long pageno;
        public ogg_int64_t packetno;  /* sequence number for decode; the framing
                             knows where there's a hole in the data,
                             but we need coupling so that the codec
                             (which is in a separate abstraction
                             layer) also knows about the gap */
        public ogg_int64_t granulepos;

    }
}