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
    /* vorbis_dsp_state buffers the current vorbis audio
       analysis/synthesis state.  The DSP state belongs to a specific
       logical bitstream ****************************************************/
    public class vorbis_dsp_state
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

        public long granulepos;
        public long sequence;

        public long glue_bits;
        public long time_bits;
        public long floor_bits;
        public long res_bits;

        public object backend_state;
    }
}