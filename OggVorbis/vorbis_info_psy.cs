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
    public class vorbis_info_psy
    {
        public int blockflag;

        public float ath_adjatt;
        public float ath_maxatt;

        public float[] tone_masteratt;
        public float tone_centerboost;
        public float tone_decay;
        public float tone_abs_limit;
        public float[] toneatt;

        public int noisemaskp;
        public float noisemaxsupp;
        public float noisewindowlo;
        public float noisewindowhi;
        public int noisewindowlomin;
        public int noisewindowhimin;
        public int noisewindowfixed;
        public float[][] noiseoff;
        public float[] noisecompand;

        public float max_curve_dB;

        public int normal_p;
        public int normal_start;
        public int normal_partition;
        public double normal_thresh;
    }
}