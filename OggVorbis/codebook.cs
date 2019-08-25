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
    public class codebook
    {
        public long dim;           /* codebook dimensions (elements per vector) */
        public long entries;       /* codebook entries */
        public long used_entries;  /* populated codebook entries */
        public static_codebook c;

        /* for encode, the below are entry-ordered, fully populated */
        /* for decode, the below are ordered by bitreversed codeword and only
           used entries are populated */
        public float[] valuelist;  /* list of dim*entries actual entry values */
        public uint[] codelist;   /* list of bitstream codewords for each entry */

        public int[] dec_index;  /* only used if sparseness collapsed */
        public char[] dec_codelengths;
        public uint[] dec_firsttable;
        public int dec_firsttablen;
        public int dec_maxlength;

        /* The current encoder uses only centered, integer-only lattice books. */
        public int quantvals;
        public int minval;
        public int delta;
    }
}