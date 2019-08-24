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
    public struct VorbisInfo
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
}