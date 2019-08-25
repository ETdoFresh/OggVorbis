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
    /* vorbis_info contains all the setup information specific to the
       specific compression/decompression mode in progress (eg,
       psychoacoustic settings, channel setup, options, codebook
       etc). vorbis_info and substructures are in backends.h.
    *********************************************************************/

    /* the comments are not part of vorbis_info so that vorbis_info can be
       static storage */
    public class vorbis_comment
    {
        /* unlimited user comment fields.  libvorbis writes 'libvorbis'
           whatever vendor is set to in encode */
        public ArrayPointer[] user_comments;
        public long[] comment_lengths;
        public long comments;
        public ArrayPointer vendor;

    }
}