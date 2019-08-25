using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OggVorbis.OS;
using static OggVorbis.ArrayPointer;
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
        public static int ogg_sync_init(ogg_sync_state oy)
        {
            oy.storage = -1; /* used as a readiness flag */
            oy.stream = new MemoryStream();
            oy.data = new ArrayPointer(oy.stream);
            oy.storage = 0;
            return (0);
        }

        public static ArrayPointer ogg_sync_buffer(ogg_sync_state oy, long size)
        {
            if (ogg_sync_check(oy) != 0) return null;

            /* first, clear out any space that has been previously returned */
            if (oy.returned != 0)
            {
                oy.fill -= oy.returned;
                if (oy.fill > 0)
                    memmove(oy.data, oy.data + oy.returned, oy.fill);
                oy.returned = 0;
            }

            if (size > oy.storage - oy.fill)
            {
                /* We need to extend the internal buffer */
                long newsize = size + oy.fill + 4096; /* an extra page to be nice */
                oy.storage = (int)newsize;
            }

            /* expose a segment at least as large as requested at the fill mark */
            return oy.data + oy.fill;
        }

        public static int ogg_sync_check(ogg_sync_state oy)
        {
            if (oy.storage < 0) return -1;
            return 0;
        }

        public static int ogg_sync_wrote(ogg_sync_state oy, long bytes)
        {
            if (ogg_sync_check(oy) != 0) return -1;
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

        public static int ogg_sync_pageout(ogg_sync_state oy, ogg_page og)
        {

            if (ogg_sync_check(oy) != 0) return 0;

            /* all we need to do is verify a page at the head of the stream
               buffer.  If it doesn't verify, we look for the next potential
               frame */

            for (; ; )
            {
                long ret = ogg_sync_pageseek(oy, og);
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
        public static long ogg_sync_pageseek(ogg_sync_state oy, ogg_page og)
        {
            ArrayPointer page = oy.data + oy.returned;
            ArrayPointer next;
            long bytes = oy.fill - oy.returned;

            if (ogg_sync_check(oy) != 0) return 0;

            if (oy.headerbytes == 0)
            {
                int headerbytes, i;
                if (bytes < 27) return (0); /* not enough for a header */

                /* verify capture pattern */
                if (memcmp(page, "OggS", 4) != 0) goto sync_fail;

                headerbytes = page[26] + 27;
                if (bytes < headerbytes) return (0); /* not enough for header + seg table */

                /* count up body length in the segment table */

                for (i = 0; i < page[26]; i++)
                    oy.bodybytes += page[27 + i];
                oy.headerbytes = headerbytes;
            }

            if (oy.bodybytes + oy.headerbytes > bytes) return (0);

            /* The whole test page is buffered.  Verify the checksum */
            {
                /* Grab the checksum bytes, set the header field to zero */
                var chksum = new byte[4];
                ogg_page log = new ogg_page();

                memcpy(chksum, page + 22, 4);
                memset(page + 22, 0, 4);

                /* set up a temp page struct and recompute the checksum */
                log.header = page;
                log.header_len = oy.headerbytes;
                log.body = page + oy.headerbytes;
                log.body_len = oy.bodybytes;
                ogg_page_checksum_set(log);

                /* Compare */
                if (memcmp(chksum, page + 22, 4) != 0)
                {
                    /* D'oh.  Mismatch! Corrupt page (or miscapture and not a page
                       at all) */
                    /* replace the computed checksum with the one actually read in */
                    memcpy(page + 22, chksum, 4);

#if DISABLE_CRC
                    /* Bad checksum. Lose sync */
                    goto sync_fail;
#endif
                }
            }

            /* yes, have a whole page all ready to go */
            {
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
            next = memchr(page + 1, 'O', bytes - 1);
            if (next == null)
                next = oy.data + oy.fill;

            oy.returned = (int)(next - oy.data);
            return ((long)-(next - page));
        }

        public static void ogg_page_checksum_set(ogg_page og)
        {
            uint crc_reg = 0;

            /* safety; needed for API behavior, but not framing code */
            og.header[22] = 0;
            og.header[23] = 0;
            og.header[24] = 0;
            og.header[25] = 0;

            crc_reg = _os_update_crc(crc_reg, og.header, og.header_len);
            crc_reg = _os_update_crc(crc_reg, og.body, og.body_len);

            og.header[22] = (byte)(crc_reg & 0xff);
            og.header[23] = (byte)((crc_reg >> 8) & 0xff);
            og.header[24] = (byte)((crc_reg >> 16) & 0xff);
            og.header[25] = (byte)((crc_reg >> 24) & 0xff);
        }

        /* checksum the page */
        /* Direct table CRC; note that this will be faster in the future if we
           perform the checksum simultaneously with other copies */

        static uint _os_update_crc(uint crc, ArrayPointer buffer, int size)
        {
            while (size >= 8)
            {
                crc ^= ((uint)buffer[0] << 24) | ((uint)buffer[1] << 16) | ((uint)buffer[2] << 8) | ((uint)buffer[3]);

                crc = crc_lookup[7][crc >> 24] ^ crc_lookup[6][(crc >> 16) & 0xFF] ^
                      crc_lookup[5][(crc >> 8) & 0xFF] ^ crc_lookup[4][crc & 0xFF] ^
                      crc_lookup[3][buffer[4]] ^ crc_lookup[2][buffer[5]] ^
                      crc_lookup[1][buffer[6]] ^ crc_lookup[0][buffer[7]];

                buffer += 8;
                size -= 8;
            }

            while (size-- > 0)
            {
                crc = (crc << 8) ^ crc_lookup[0][((crc >> 24) & 0xff) ^ buffer[0]];
                buffer += 1;
            }
            return crc;
        }

        /* init the encode/decode logical stream state */

        public static int ogg_stream_init(ogg_stream_state os, int serialno)
        {
            os.body_storage = 16 * 1024;
            os.lacing_storage = 1024;

            os.stream = new MemoryStream();
            os.body_data = new ArrayPointer(os.stream);      // _ogg_malloc(os.body_storage * sizeof(*os.body_data));
            os.lacing_vals = new int[os.lacing_storage];   // _ogg_malloc(os.lacing_storage * sizeof(*os.lacing_vals));
            os.granule_vals = new long[os.lacing_storage]; // _ogg_malloc(os.lacing_storage * sizeof(*os.granule_vals));

            if (os.body_data == null || os.lacing_vals == null || os.granule_vals == null)
            {
                ogg_stream_clear(os);
                return -1;
            }

            os.serialno = serialno;

            return (0);
        }

        /* _clear does not free os, only the non-flat storage within */
        public static int ogg_stream_clear(ogg_stream_state os)
        {
            os.body_data = null;
            os.lacing_vals = null;
            os.granule_vals = null;
            return (0);
        }

        /* clear non-flat storage within */
        public static int ogg_sync_clear(ogg_sync_state oy)
        {
            oy.stream.SetLength(0);
            return (0);
        }

        /* add the incoming page to the stream state; we decompose the page
   into packet segments here as well. */

        public static int ogg_stream_pagein(ogg_stream_state os, ogg_page og)
        {
            ArrayPointer header = og.header;
            ArrayPointer body = og.body;
            long bodysize = og.body_len;
            int segptr = 0;

            int version = ogg_page_version(og);
            int continued = ogg_page_continued(og);
            int bos = ogg_page_bos(og);
            int eos = ogg_page_eos(og);
            long granulepos = ogg_page_granulepos(og);
            int serialno = ogg_page_serialno(og);
            long pageno = ogg_page_pageno(og);
            int segments = header[26];

            if (ogg_stream_check(os) != 0) return -1;

            /* clean up 'returned data' */
            {
                long lr = os.lacing_returned;
                long br = os.body_returned;

                /* body data */
                if (br != 0)
                {
                    os.body_fill -= br;
                    if (os.body_fill != 0)
                        memmove(os.body_data, os.body_data + br, os.body_fill);
                    os.body_returned = 0;
                }

                if (lr != 0)
                {
                    /* segment table */
                    if ((os.lacing_fill - lr) != 0)
                    {
                        memmove(os.lacing_vals, /*os.lacing_vals +*/ lr,
                                (os.lacing_fill - lr) /* * sizeof(*os.lacing_vals)*/);
                        memmove(os.granule_vals, /*os.granule_vals +*/ lr,
                                (os.lacing_fill - lr) /* * sizeof(*os.granule_vals)*/);
                    }
                    os.lacing_fill -= lr;
                    os.lacing_packet -= lr;
                    os.lacing_returned = 0;
                }
            }

            /* check the serial number */
            if (serialno != os.serialno) return (-1);
            if (version > 0) return (-1);

            if (_os_lacing_expand(os, segments + 1) != 0) return -1;

            /* are we in sequence? */
            if (pageno != os.pageno)
            {
                long i;

                /* unroll previous partial packet (if any) */
                for (i = os.lacing_packet; i < os.lacing_fill; i++)
                    os.body_fill -= os.lacing_vals[i] & 0xff;
                os.lacing_fill = os.lacing_packet;

                /* make a note of dropped data in segment table */
                if (os.pageno != -1)
                {
                    os.lacing_vals[os.lacing_fill++] = 0x400;
                    os.lacing_packet++;
                }
            }

            /* are we a 'continued packet' page?  If so, we may need to skip
               some segments */
            if (continued != 0)
            {
                if (os.lacing_fill < 1 ||
                   (os.lacing_vals[os.lacing_fill - 1] & 0xff) < 255 ||
                   os.lacing_vals[os.lacing_fill - 1] == 0x400)
                {
                    bos = 0;
                    for (; segptr < segments; segptr++)
                    {
                        int val = header[27 + segptr];
                        body += val;
                        bodysize -= val;
                        if (val < 255)
                        {
                            segptr++;
                            break;
                        }
                    }
                }
            }

            if (bodysize != 0)
            {
                if (_os_body_expand(os, bodysize) != 0) return -1;
                memcpy(os.body_data + os.body_fill, body, bodysize);
                os.body_fill += bodysize;
            }

            {
                long saved = -1;
                while (segptr < segments)
                {
                    int val = header[27 + segptr];
                    os.lacing_vals[os.lacing_fill] = val;
                    os.granule_vals[os.lacing_fill] = -1;

                    if (bos != 0)
                    {
                        os.lacing_vals[os.lacing_fill] |= 0x100;
                        bos = 0;
                    }

                    if (val < 255) saved = os.lacing_fill;

                    os.lacing_fill++;
                    segptr++;

                    if (val < 255) os.lacing_packet = os.lacing_fill;
                }

                /* set the granulepos on the last granuleval of the last full packet */
                if (saved != -1)
                {
                    os.granule_vals[saved] = granulepos;
                }

            }

            if (eos != 0)
            {
                os.e_o_s = 1;
                if (os.lacing_fill > 0)
                    os.lacing_vals[os.lacing_fill - 1] |= 0x200;
            }

            os.pageno = pageno + 1;

            return (0);
        }

        /* A complete description of Ogg framing exists in docs/framing.html */

        public static int ogg_page_version(ogg_page og)
        {
            return ((int)(og.header[4]));
        }

        public static int ogg_page_continued(ogg_page og)
        {
            return ((int)(og.header[5] & 0x01));
        }

        public static int ogg_page_bos(ogg_page og)
        {
            return ((int)(og.header[5] & 0x02));
        }

        public static int ogg_page_eos(ogg_page og)
        {
            return ((int)(og.header[5] & 0x04));
        }

        public static long ogg_page_granulepos(ogg_page og)
        {
            ArrayPointer page = og.header;
            long granulepos = page[13] & (0xff);
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            granulepos = (granulepos << 8) | (page[12] & 0xff);
            granulepos = (granulepos << 8) | (page[11] & 0xff);
            granulepos = (granulepos << 8) | (page[10] & 0xff);
            granulepos = (granulepos << 8) | (page[9] & 0xff);
            granulepos = (granulepos << 8) | (page[8] & 0xff);
            granulepos = (granulepos << 8) | (page[7] & 0xff);
            granulepos = (granulepos << 8) | (page[6] & 0xff);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            return ((long)granulepos);
        }

        public static int ogg_page_serialno(ogg_page og)
        {
            return og.header[14] |
                og.header[15] << 8 |
                og.header[16] << 16 |
                og.header[17] << 24;
        }

        public static long ogg_page_pageno(ogg_page og)
        {
            return og.header[18] |
                og.header[19] << 8 |
                og.header[20] << 16 |
                og.header[21] << 24;
        }

        /* async/delayed error detection for the ogg_stream_state */
        public static int ogg_stream_check(ogg_stream_state os)
        {
            if (os == null || os.body_data == null) return -1;
            return 0;
        }

        public static int _os_lacing_expand(ogg_stream_state os, long needed)
        {
            if (os.lacing_storage - needed <= os.lacing_fill)
            {
                long lacing_storage;
                object ret;
                if (os.lacing_storage > long.MaxValue - needed)
                {
                    ogg_stream_clear(os);
                    return -1;
                }
                lacing_storage = os.lacing_storage + needed;
                if (lacing_storage < long.MaxValue - 32) lacing_storage += 32;
                ret = _ogg_realloc(os.lacing_vals, lacing_storage /* * sizeof(*os.lacing_vals)*/);
                if (ret == null)
                {
                    ogg_stream_clear(os);
                    return -1;
                }
                os.lacing_vals = (int[])ret;
                ret = _ogg_realloc(os.granule_vals, lacing_storage /* * sizeof(*os.granule_vals)*/);
                if (ret == null)
                {
                    ogg_stream_clear(os);
                    return -1;
                }
                os.granule_vals = (long[])ret;
                os.lacing_storage = lacing_storage;
            }
            return 0;
        }

        /* Helpers for ogg_stream_encode; this keeps the structure and
           what's happening fairly clear */

        public static int _os_body_expand(ogg_stream_state os, long needed)
        {
            if (os.body_storage - needed <= os.body_fill)
            {
                long body_storage;
                //object ret;
                if (os.body_storage > long.MaxValue - needed)
                {
                    ogg_stream_clear(os);
                    return -1;
                }
                body_storage = os.body_storage + needed;
                // Uses an expanding array, hence reallocation should not be necessary
                //if (body_storage < long.MaxValue - 1024) body_storage += 1024;
                //ret = _ogg_realloc(os.body_data, body_storage /* * sizeof(*os.body_data)*/);
                //if (ret == null)
                //{
                //    ogg_stream_clear(os);
                //    return -1;
                //}
                os.body_storage = body_storage;
                //os.body_data = ret;
            }
            return 0;
        }
    }
}
