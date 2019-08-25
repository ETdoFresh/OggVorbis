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
    public class static_codebook
    {
        public long dim;           /* codebook dimensions (elements per vector) */
        public long entries;       /* codebook entries */
        public long[] lengthlist;  /* codeword lengths in bits */

        /* mapping ***************************************************************/
        public int maptype;       /* 0=none
                                    1=implicitly populated values from map column
                                    2=listed arbitrary values */

        /* The below does a linear, single monotonic sequence mapping. */
        public long q_min;      /* packed 32 bit float; quant value 0 maps to minval */
        public long q_delta;    /* packed 32 bit float; val 1 - val 0 == delta */
        public int q_quant;     /* bits: 0 < quant <= 16 */
        public int q_sequencep; /* bitflag */

        public long[] quantlist;  /* map == 1: (int)(entries^(1/dim)) element column map
                           map == 2: list of dim*entries quantized entry vals
                        */
        public int allocedp;
    }
}