/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: libvorbis codec headers
  
 Ported by: ETdoFresh

 ********************************************************************/
namespace OggVorbis
{
    public class codec_internal
    {
        public const int BLOCKTYPE_IMPULSE = 0;
        public const int BLOCKTYPE_PADDING = 1;
        public const int BLOCKTYPE_TRANSITION = 0;
        public const int BLOCKTYPE_LONG = 1;

        public const int PACKETBLOBS = 15;



        public struct vorbis_block_internal
        {
            public float[][] pcmdelay;  /* this is a pointer into local storage */
            public float ampmax;
            public int blocktype;

            public oggpack_buffer* packetblob[PACKETBLOBS]; /* initialized, must be freed;
                                              blob [PACKETBLOBS/2] points to
                                              the oggpack_buffer in the
                                              main vorbis_block */
        }

        public void vorbis_look_floor;
        public void vorbis_look_residue;
        public void vorbis_look_transform;

        /* mode ************************************************************/
        public struct vorbis_info_mode
        {
            public int blockflag;
            public int windowtype;
            public int transformtype;
            public int mapping;
        }

        public void vorbis_info_floor;
        public void vorbis_info_residue;
        public void vorbis_info_mapping;

        public struct private_state
        {
            /* local lookup storage */
            public envelope_lookup* ve; /* envelope lookup */
            public int window[2];
            public vorbis_look_transform** transform[2];    /* block, type */
            public drft_lookup fft_look[2];

            public int modebits;
            public vorbis_look_floor** flr;
            public vorbis_look_residue** residue;
            public vorbis_look_psy* psy;
            public vorbis_look_psy_global* psy_g_look;

            /* local storage, only used on the encoding side.  This way the
               application does not need to worry about freeing some packets'
               memory and not others'; packet storage is always tracked.
               Cleared next call to a _dsp_ function */
            public unsigned char* header;
            public unsigned char* header1;
            public unsigned char* header2;

            public bitrate_manager_state bms;

            public ogg_int64_t sample_count;
        }

        /* codec_setup_info contains all the setup information specific to the
           specific compression/decompression mode in progress (eg,
           psychoacoustic settings, channel setup, options, codebook
           etc).
        *********************************************************************/

        public struct codec_setup_info
        {

            /* Vorbis supports only short and long blocks, but allows the
               encoder to choose the sizes */

            public long[] blocksizes/*[2]*/;

            /* modes are the primary means of supporting on-the-fly different
               blocksizes, different channel mappings (LR or M/A),
               different residue backends, etc.  Each mode consists of a
               blocksize flag and a mapping (along with the mapping setup */

            public int modes;
            public int maps;
            public int floors;
            public int residues;
            public long books;
            public int psys;     /* encode only */

            public vorbis_info_mode[] mode_param/*[64]*/;
            public int[] map_type/*[64]*/;
            public vorbis_info_mapping[] map_param/*[64]*/;
            public int[] floor_type/*[64]*/;
            public vorbis_info_floor[] floor_param/*[64]*/;
            public int[] residue_type/*[64]*/;
            public vorbis_info_residue[] residue_param/*[64]*/;
            public static_codebook[] book_param/*[256]*/;
            public codebook[] fullbooks;

            public vorbis_info_psy[] psy_param/*[4]*/; /* encode only */
            public vorbis_info_psy_global psy_g_param;

            public bitrate_manager_info bi;
            public highlevel_encode_setup hi; /* used only by vorbisenc.c.  It's a
                                highly redundant structure, but
                                improves clarity of program flow. */
            public int halfrate_flag; /* painless downsample for decode */
        }

        public struct vorbis_look_floor1
        {
            public int sorted_index[VIF_POSIT + 2];
            public int forward_index[VIF_POSIT + 2];
            public int reverse_index[VIF_POSIT + 2];

            public int hineighbor[VIF_POSIT];
            public int loneighbor[VIF_POSIT];
            public int posts;

            public int n;
            public int quant_q;
            public vorbis_info_floor1* vi;

            public long phrasebits;
            public long postbits;
            public long frames;
        }
    }
}