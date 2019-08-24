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

 function: simple example decoder
  
 Ported by: ETdoFresh [2019]

 ********************************************************************/
using System;
using static OggVorbis.Framing;
using static OggVorbis.OS;
using ogg_int16_t = System.Int16;
using System.IO;

/* Takes a vorbis bitstream from stdin and writes raw stereo PCM to
   stdout. Decodes simple and chained OggVorbis files from beginning
   to end. Vorbisfile.a is somewhat more complex than the code below.  */
namespace OggVorbis
{
    public class DecoderExample
    {
        /* Note that this is POSIX, not ANSI code */
        static ushort[] convbuffer = new ushort[4096]; /* take 8k out of the data segment, not the stack */
        static long convsize = 4096;

        public static void Main(string[] args)
        {
            /* sync and verify incoming physical bitstream */
            var oggSyncState = new ogg_sync_state();

            /* take physical pages, weld into a logical stream of packets */
            var oggStreamState = new OggStreamState();

            /* one Ogg bitstream page. Vorbis packets are inside */
            var oggPage = new ogg_page();

            /* one raw packet of data for decode */
            var oggPacket = new OggPacket();

            /* struct that stores all the static vorbis bitstream settings */
            var vorbisInfo = new VorbisInfo();

            /* struct that stores all the bitstream user comments */
            var vorbisComment = new VorbisComment();

            /* central working state for the packet->PCM decoder */
            var vorbisDspState = new VorbisDspState();

            /* local working space for packet->PCM decode */
            var vorbisBlock = new VorbisBlock();

            var stdin = Console.OpenStandardInput();
            var stdout = Console.OpenStandardOutput();
            var stderr = Console.OpenStandardError();

            /********** Decode setup ************/

            ogg_sync_init(ref oggSyncState); /* Now we can read pages */

            while (true)
            { /* we repeat if the bitstream is chained */

                /* grab some data at the head of the stream. We want the first page
                   (which is guaranteed to be small and only contain the Vorbis
                   stream initial header) We need the first page to get the stream
                   serialno. */

                /* submit a 4k block to libvorbis' Ogg layer */
                ogg_sync_buffer(ref oggSyncState, 4096);
                int bytes = fread(ref oggSyncState, 1, 4096, stdin);
                ogg_sync_wrote(ref oggSyncState, bytes);

                /* Get the first page. */
                if (ogg_sync_pageout(ref oggSyncState, ref oggPage) != 1)
                {
                    /* have we simply run out of data?  If so, we're done. */
                    if (bytes < 4096) break;

                    /* error case.  Must not be Vorbis data */
                    fprintf(stderr, "Input does not appear to be an Ogg bitstream.\n");
                    exit(1);
                }
                
                else { break; } // TODO: ERASE!! DEBUG LINE!!

////                /* Get the serial number and set up the rest of decode. */
////                /* serialno first; use it to set up a logical stream */
////                ogg_stream_init(oggStreamState, ogg_page_serialno(oggPage));

////                /* extract the initial header from the first page and verify that the
////                   Ogg bitstream is in fact Vorbis data */

////                /* I handle the initial header first instead of just having the code
////                   read all three Vorbis headers at once because reading the initial
////                   header is an easy way to identify a Vorbis bitstream and it's
////                   useful to see that functionality seperated out. */

////                vorbis_info_init(vorbisInfo);
////                vorbis_comment_init(vorbisComment);
////                if (ogg_stream_pagein(oggStreamState, oggPage) < 0)
////                {
////                    /* error; stream version mismatch perhaps */
////                    fprintf(stderr, "Error reading first page of Ogg bitstream data.\n");
////                    exit(1);
////                }

////                if (ogg_stream_packetout(oggStreamState, oggPacket) != 1)
////                {
////                    /* no page? must not be vorbis */
////                    fprintf(stderr, "Error reading initial header packet.\n");
////                    exit(1);
////                }

////                if (vorbis_synthesis_headerin(vorbisInfo, vorbisComment, oggPacket) < 0)
////                {
////                    /* error case; not a vorbis header */
////                    fprintf(stderr, "This Ogg bitstream does not contain Vorbis audio data.\n");
////                    exit(1);
////                }

////                /* At this point, we're sure we're Vorbis. We've set up the logical
////                   (Ogg) bitstream decoder. Get the comment and codebook headers and
////                   set up the Vorbis decoder */

////                /* The next two packets in order are the comment and codebook headers.
////                   They're likely large and may span multiple pages. Thus we read
////                   and submit data until we get our two packets, watching that no
////                   pages are missing. If a page is missing, error out; losing a
////                   header page is the only place where missing data is fatal. */

////                i = 0;
////                while (i < 2)
////                {
////                    while (i < 2)
////                    {
////                        int result = ogg_sync_pageout(oggSyncState, oggPage);
////                        if (result == 0) break; /* Need more data */
////                                                /* Don't complain about missing or corrupt data yet. We'll
////                                                   catch it at the packet output phase */
////                        if (result == 1)
////                        {
////                            ogg_stream_pagein(oggStreamState, oggPage); /* we can ignore any errors here
////                                         as they'll also become apparent
////                                         at packetout */
////                            while (i < 2)
////                            {
////                                result = ogg_stream_packetout(oggStreamState, oggPacket);
////                                if (result == 0) break;
////                                if (result < 0)
////                                {
////                                    /* Uh oh; data at some point was corrupted or missing!
////                                       We can't tolerate that in a header.  Die. */
////                                    fprintf(stderr, "Corrupt secondary header.  Exiting.\n");
////                                    exit(1);
////                                }
////                                result = vorbis_synthesis_headerin(vorbisInfo, vorbisComment, oggPacket);
////                                if (result < 0)
////                                {
////                                    fprintf(stderr, "Corrupt secondary header.  Exiting.\n");
////                                    exit(1);
////                                }
////                                i++;
////                            }
////                        }
////                    }
////                    /* no harm in not checking before adding more */
////                    buffer = ogg_sync_buffer(oggSyncState, 4096);
////                    bytes = fread(buffer, 1, 4096, stdin);
////                    if (bytes == 0 && i < 2)
////                    {
////                        fprintf(stderr, "End of file before finding all Vorbis headers!\n");
////                        exit(1);
////                    }
////                    ogg_sync_wrote(oggSyncState, bytes);
////                }

////                /* Throw the comments plus a few lines about the bitstream we're
////                   decoding */
////                {
////                    char** ptr = vorbisComment.user_comments;
////                    while (*ptr)
////                    {
////                        fprintf(stderr, "%s\n", *ptr);
////                        ++ptr;
////                    }
////                    fprintf(stderr, "\nBitstream is %d channel, %ldHz\n", vorbisInfo.channels, vorbisInfo.rate);
////                    fprintf(stderr, "Encoded by: %s\n\n", vorbisComment.vendor);
////                }

////                convsize = 4096 / vorbisInfo.channels;

////                /* OK, got and parsed all three headers. Initialize the Vorbis
////                   packet->PCM decoder. */
////                if (vorbis_synthesis_init(vorbisDspState, vorbisInfo) == 0)
////                { /* central decode state */
////                    vorbis_block_init(vorbisDspState, vorbisBlock);          /* local state for most of the decode
////                                              so multiple block decodes can
////                                              proceed in parallel. We could init
////                                              multiple vorbis_block structures
////                                              for vd here */

////                    /* The rest is just a straight decode loop until end of stream */
////                    while (eos == 0)
////                    {
////                        while (eos == 0)
////                        {
////                            int result = ogg_sync_pageout(oggSyncState, oggPage);
////                            if (result == 0) break; /* need more data */
////                            if (result < 0)
////                            { /* missing or corrupt data at this page position */
////                                fprintf(stderr, "Corrupt or missing data in bitstream; "



////                                        "continuing...\n");
////                            }
////                            else
////                            {
////                                ogg_stream_pagein(oggStreamState, oggPage); /* can safely ignore errors at
////                                           this point */
////                                while (1)
////                                {
////                                    result = ogg_stream_packetout(&oggStreamState, &oggPacket);

////                                    if (result == 0) break; /* need more data */
////                                    if (result < 0)
////                                    { /* missing or corrupt data at this page position */
////                                      /* no reason to complain; already complained above */
////                                    }
////                                    else
////                                    {
////                                        /* we have a packet.  Decode it */
////                                        float** pcm;
////                                        int samples;

////                                        if (vorbis_synthesis(&vorbisBlock, &oggPacket) == 0) /* test for success! */
////                                            vorbis_synthesis_blockin(&vorbisDspState, &vorbisBlock);
////                                        /*

////                                        **pcm is a multichannel float vector.  In stereo, for
////                                        example, pcm[0] is left, and pcm[1] is right.  samples is
////                                        the size of each channel.  Convert the float values
////                                        (-1.<=range<=1.) to whatever PCM format and write it out */

////                                        while ((samples = vorbis_synthesis_pcmout(&vorbisDspState, &pcm)) > 0)
////                                        {
////                                            int j;
////                                            int clipflag = 0;
////                                            int bout = (samples < convsize ? samples : convsize);

////                                            /* convert floats to 16 bit signed ints (host order) and
////                                               interleave */
////                                            for (i = 0; i < vorbisInfo.channels; i++)
////                                            {
////                                                ogg_int16_t* ptr = convbuffer + i;
////                                                float* mono = pcm[i];
////                                                for (j = 0; j < bout; j++)
////                                                {
////#if 1
////                      int val=floor(mono[j]*32767.f+.5f);
////#else /* optional dither */
////                                                    int val = mono[j] * 32767.f + drand48() - 0.5f;
////#endif
////                                                    /* might as well guard against clipping */
////                                                    if (val > 32767)
////                                                    {
////                                                        val = 32767;
////                                                        clipflag = 1;
////                                                    }
////                                                    if (val < -32768)
////                                                    {
////                                                        val = -32768;
////                                                        clipflag = 1;
////                                                    }
////                                                    *ptr = val;
////                                                    ptr += vorbisInfo.channels;
////                                                }
////                                            }

////                                            if (clipflag)
////                                                fprintf(stderr, "Clipping in frame %ld\n", (long)(vorbisDspState.sequence));


////                                            fwrite(convbuffer, 2 * vorbisInfo.channels, bout, stdout);

////                                            vorbis_synthesis_read(vorbisDspState, bout); /* tell libvorbis how
////                                                      many samples we
////                                                      actually consumed */
////                                        }
////                                    }
////                                }
////                                if (ogg_page_eos(oggPage) != 0) eos = 1;
////                            }
////                        }
////                        if (eos == 0)
////                        {
////                            buffer = ogg_sync_buffer(oggSyncState, 4096);
////                            bytes = fread(buffer, 1, 4096, stdin);
////                            ogg_sync_wrote(oggSyncState, bytes);
////                            if (bytes == 0) eos = 1;
////                        }
////                    }

////                    /* ogg_page and ogg_packet structs always point to storage in
////                       libvorbis.  They're never freed or manipulated directly */

////                    vorbis_block_clear(vorbisBlock);
////                    vorbis_dsp_clear(vorbisDspState);
////                }
////                else
////                {
////                    fprintf(stderr, "Error: Corrupt header during playback initialization.\n");
////                }

////                /* clean up this logical bitstream; before exit we see if we're
////                   followed by another [chained] */

////                ogg_stream_clear(oggStreamState);
////                vorbis_comment_clear(vorbisComment);
////                vorbis_info_clear(vorbisInfo);  /* must be called last */
            }

            /* OK, clean up the framer */
            ogg_sync_clear(oggSyncState);

            fprintf(stderr, "Done.\n");
        }
    }
}