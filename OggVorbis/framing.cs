using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OggVorbis.OS;
using static OggVorbis.crctable;

namespace OggVorbis
{
    public class Framing
    {
        /* DECODING PRIMITIVES: packet streaming layer **********************/

        /* This has two layers to place more of the multi-serialno and paging
           control in the application's hands.  First, we expose a data buffer
           using ogg_sync_buffer().  The app either copies into the
           buffer, or passes it directly to read(), etc.  We then call
           ogg_sync_wrote() to tell how many bytes we just added.

           Pages are returned (pointers into the buffer in ogg_sync_state)
           by ogg_sync_pageout().  The page is then submitted to
           ogg_stream_pagein() along with the appropriate
           ogg_stream_state* (ie, matching serialno).  We then get raw
           packets out calling ogg_stream_packetout() with a
           ogg_stream_state. */

        /* initialize the struct to a known state */
        public static int ogg_sync_init(ref ogg_sync_state oy)
        {
            oy.storage = -1; /* used as a readiness flag */
            oy.stream = new MemoryStream();
            oy.storage = 0;
            return (0);
        }

        public static int ogg_sync_buffer(ref ogg_sync_state oy, long size)
        {
            if (ogg_sync_check(ref oy) != 0) return -1;

            /* first, clear out any space that has been previously returned */
            if (oy.returned != 0)
            {
                oy.fill -= oy.returned;
                if (oy.fill > 0)
                    memmove(oy.stream, oy.data, oy.data + oy.returned, oy.fill);
                oy.returned = 0;
            }

            if (size > oy.storage - oy.fill)
            {
                /* We need to extend the internal buffer */
                long newsize = size + oy.fill + 4096; /* an extra page to be nice */

                // Stream does no need to reallocate memory //

                //int ret;


                //if (oy.data)
                //    ret = _ogg_realloc(oy.data, newsize);
                //else
                //    ret = _ogg_malloc(newsize);

                //if (!ret)
                //{
                //    ogg_sync_clear(oy);
                //    return NULL;
                //}
                //oy.data = ret;

                oy.storage = (int)newsize;
            }

            /* expose a segment at least as large as requested at the fill mark */
            return oy.data + oy.fill;
        }

        public static int ogg_sync_check(ref ogg_sync_state oy)
        {
            if (oy.storage < 0) return -1;
            return 0;
        }

        public static int ogg_sync_wrote(ref ogg_sync_state oy, long bytes)
        {
            if (ogg_sync_check(ref oy) != 0) return -1;
            if (oy.fill + bytes > oy.storage) return -1;
            oy.fill += (int)bytes;
            return (0);
        }

        /* sync the stream and get a page.  Keep trying until we find a page.
           Suppress 'sync errors' after reporting the first.

           return values:
           -1) recapture (hole in data)
            0) need more data
            1) page returned

           Returns pointers into buffered data; invalidated by next call to
           _stream, _clear, _init, or _buffer */

        public static int ogg_sync_pageout(ref ogg_sync_state oy, ref ogg_page og)
        {

            if (ogg_sync_check(ref oy) != 0) return 0;

            /* all we need to do is verify a page at the head of the stream
               buffer.  If it doesn't verify, we look for the next potential
               frame */

            for (; ; )
            {
                long ret = ogg_sync_pageseek(ref oy, ref og);
                if (ret > 0)
                {
                    /* have a page */
                    return (1);
                }
                if (ret == 0)
                {
                    /* need more data */
                    return (0);
                }

                /* head did not start a synced page... skipped some bytes */
                if (oy.unsynced == 0)
                {
                    oy.unsynced = 1;
                    return (-1);
                }

                /* loop. keep looking */

            }
        }

        /* sync the stream.  This is meant to be useful for finding page
           boundaries.

           return values for this:
          -n) skipped n bytes
           0) page not ready; more data (no bytes skipped)
           n) page synced at current location; page length n bytes

        */
        public static long ogg_sync_pageseek(ref ogg_sync_state oy, ref ogg_page og)
        {
            int page = oy.data + oy.returned;
            int next;
            long bytes = oy.fill - oy.returned;

            if (ogg_sync_check(ref oy) != 0) return 0;

            if (oy.headerbytes == 0)
            {
                int headerbytes, i;
                if (bytes < 27) return (0); /* not enough for a header */

                /* verify capture pattern */
                if (memcmp(oy.stream, page, "OggS", 4) != 0) goto sync_fail;

                headerbytes = oy.stream.ReadAt(page + 26) + 27;
                if (bytes < headerbytes) return (0); /* not enough for header + seg table */

                /* count up body length in the segment table */

                for (i = 0; i < oy.stream.ReadAt(page + 26); i++)
                    oy.bodybytes += oy.stream.ReadAt(page + 27 + i);
                oy.headerbytes = headerbytes;
            }

            if (oy.bodybytes + oy.headerbytes > bytes) return (0);

            /* The whole test page is buffered.  Verify the checksum */
            {
                /* Grab the checksum bytes, set the header field to zero */
                var chksum = new byte[4];
                ogg_page log = new ogg_page();

                oy.stream.memcpy(chksum, page + 22, 4);
                oy.stream.memset(page + 22, 0, 4);

                /* set up a temp page struct and recompute the checksum */
                log.stream = oy.stream;
                log.header = page;
                log.header_len = oy.headerbytes;
                log.body = page + oy.headerbytes;
                log.body_len = oy.bodybytes;
                ogg_page_checksum_set(ref log);

                /* Compare */
                if (oy.stream.memcmp(chksum, page + 22, 4) != 0)
                {
                    /* D'oh.  Mismatch! Corrupt page (or miscapture and not a page
                       at all) */
                    /* replace the computed checksum with the one actually read in */
                    oy.stream.memcpy(page + 22, chksum, 4);

#if DISABLE_CRC
                    /* Bad checksum. Lose sync */
                    goto sync_fail;
#endif
                }
            }

            /* yes, have a whole page all ready to go */
            {
                og.stream = oy.stream;
                og.header = page;
                og.header_len = oy.headerbytes;
                og.body = page + oy.headerbytes;
                og.body_len = oy.bodybytes;

                oy.unsynced = 0;
                oy.returned += (int)(bytes = oy.headerbytes + oy.bodybytes);
                oy.headerbytes = 0;
                oy.bodybytes = 0;
                return (bytes);
            }

            sync_fail:

            oy.headerbytes = 0;
            oy.bodybytes = 0;

            /* search for possible capture */
            next = oy.stream.memchr(page + 1, Convert.ToByte('O'), (int)(bytes - 1));
            if (next == 0)
                next = oy.data + oy.fill;

            oy.returned = (int)(next - oy.data);
            return ((long)-(next - page));
        }

        public static void ogg_page_checksum_set(ref ogg_page og)
        {
            uint crc_reg = 0;

            /* safety; needed for API behavior, but not framing code */
            og.stream.WriteAt(22, 0);
            og.stream.WriteAt(23, 0);
            og.stream.WriteAt(24, 0);
            og.stream.WriteAt(25, 0);

            crc_reg = _os_update_crc(crc_reg, og.stream, og.header, og.header_len);
            crc_reg = _os_update_crc(crc_reg, og.stream, og.body, og.body_len);

            og.stream.WriteAt(22, (byte)(crc_reg & 0xff));
            og.stream.WriteAt(23, (byte)((crc_reg >> 8) & 0xff));
            og.stream.WriteAt(24, (byte)((crc_reg >> 16) & 0xff));
            og.stream.WriteAt(25, (byte)((crc_reg >> 24) & 0xff));
        }

        /* checksum the page */
        /* Direct table CRC; note that this will be faster in the future if we
           perform the checksum simultaneously with other copies */

        static uint _os_update_crc(uint crc, MemoryStream stream, int buffer, int size)
        {
            while (size >= 8)
            {
                crc ^= ((uint)stream.ReadAt(buffer + 0) << 24) | ((uint)stream.ReadAt(buffer + 1) << 16) | ((uint)stream.ReadAt(buffer + 2) << 8) | ((uint)stream.ReadAt(buffer + 3));

                crc = crc_lookup[7][crc >> 24] ^ crc_lookup[6][(crc >> 16) & 0xFF] ^
                      crc_lookup[5][(crc >> 8) & 0xFF] ^ crc_lookup[4][crc & 0xFF] ^
                      crc_lookup[3][stream.ReadAt(buffer + 4)] ^ crc_lookup[2][stream.ReadAt(buffer + 5)] ^
                      crc_lookup[1][stream.ReadAt(buffer + 6)] ^ crc_lookup[0][stream.ReadAt(buffer + 7)];

                buffer += 8;
                size -= 8;
            }

            while (size-- > 0)
            {
                crc = (crc << 8) ^ crc_lookup[0][((crc >> 24) & 0xff) ^ stream.ReadAt(buffer)];
                buffer++;
            }
            return crc;
        }

        /* clear non-flat storage within */
        public static int ogg_sync_clear(ogg_sync_state oy)
        {
            oy.stream.SetLength(0);
            return (0);
        }
    }
}
