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
namespace OggVorbis
{
    public class codec_setup_info
    {
        /* Vorbis supports only short and long blocks, but allows the
           encoder to choose the sizes */

        public long[] blocksizes;

        /* modes are the primary means of supporting on-the-fly different
           blocksizes, different channel mappings (LR or M/A),
           different residue backends, etc.  Each mode consists of a
           blocksize flag and a mapping (along with the mapping setup */

        public int modes;
        public int maps;
        public int floors;
        public int residues;
        public int books;
        public int psys;     /* encode only */

        public vorbis_info_mode[] mode_param;
        public int[] map_type;
        public vorbis_info_mapping[] map_param;
        public int[] floor_type;
        public vorbis_info_floor[] floor_param;
        public int[] residue_type;
        public vorbis_info_residue[] residue_param;
        public static_codebook[] book_param;
        public codebook fullbooks;

        public vorbis_info_psy[] psy_param; /* encode only */
        public vorbis_info_psy_global psy_g_param;

        public bitrate_manager_info bi;
        public highlevel_encode_setup hi; /* used only by vorbisenc.c.  It's a
                                highly redundant structure, but
                                improves clarity of program flow. */
        public int halfrate_flag; /* painless downsample for decode */
    }
}