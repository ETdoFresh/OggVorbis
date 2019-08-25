using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OggVorbis.ArrayPointer;
using static OggVorbis.bitwise;
using static OggVorbis.codec;
using static OggVorbis.registry;
using static OggVorbis.sharedbook;

namespace OggVorbis
{
    public class Info
    {
        public static string GENERAL_VENDOR_STRING = "Xiph.Org libVorbis 1.3.6";
        public static string ENCODE_VENDOR_STRING = "Xiph.Org libVorbis I 20180316 (Now 100% fewer shells)";

        /* used by synthesis, which has a full, alloced vi */
        public static void vorbis_info_init(vorbis_info vi)
        {
            vi.codec_setup = new codec_setup_info();
        }

        public static void vorbis_comment_init(vorbis_comment vc)
        {
        }

        /* The Vorbis header is in three packets; the initial small packet in
           the first page that identifies basic parameters, a second packet
           with bitstream comments and a third packet that holds the
           codebook. */

        public static int vorbis_synthesis_headerin(vorbis_info vi, vorbis_comment vc, ogg_packet op)
        {
            oggpack_buffer opb = new oggpack_buffer();

            if (op != null)
            {
                oggpack_readinit(opb, op.packet, op.bytes);

                /* Which of the three types of header is this? */
                /* Also verify header-ness, vorbis */
                {
                    var buffer = new ArrayPointer();
                    long packtype = oggpack_read(opb, 8);
                    //memset(buffer, 0, 6);
                    _v_readstring(opb, buffer, 6);
                    if (memcmp(buffer, "vorbis", 6) != 0)
                    {
                        /* not a vorbis header */
                        return (OV_ENOTVORBIS);
                    }
                    switch (packtype)
                    {
                        case 0x01: /* least significant *bit* is read first */
                            if (op.b_o_s == 0)
                            {
                                /* Not the initial packet */
                                return (OV_EBADHEADER);
                            }
                            if (vi.rate != 0)
                            {
                                /* previously initialized info header */
                                return (OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_info(vi, opb));

                        case 0x03: /* least significant *bit* is read first */
                            if (vi.rate == 0)
                            {
                                /* um... we didn't get the initial header */
                                return (OV_EBADHEADER);
                            }
                            if (vc.vendor != null)
                            {
                                /* previously initialized comment header */
                                return (OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_comment(vc, opb));

                        case 0x05: /* least significant *bit* is read first */
                            if (vi.rate == 0 || vc.vendor == null)
                            {
                                /* um... we didn;t get the initial header or comments yet */
                                return (OV_EBADHEADER);
                            }
                            if (vi.codec_setup == null)
                            {
                                /* improperly initialized vorbis_info */
                                return (OV_EFAULT);
                            }
                            if (((codec_setup_info)vi.codec_setup).books > 0)
                            {
                                /* previously initialized setup header */
                                return (OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_books(vi, opb));

                        default:
                            /* Not a valid vorbis header type */
                            return (OV_EBADHEADER);
                            //break;
                    }
                }
            }
            return (OV_EBADHEADER);
        }

        public static void _v_readstring(oggpack_buffer o, ArrayPointer buf, int bytes)
        {
            var i = 0;
            while (bytes-- != 0)
            {
                buf[i] = (byte)oggpack_read(o, 8);
                i++;
            }
        }

        /* Header packing/unpacking ********************************************/

        public static int _vorbis_unpack_info(vorbis_info vi, oggpack_buffer opb)
        {
            codec_setup_info ci = vi.codec_setup;
            int bs;
            if (ci == null) return (OV_EFAULT);

            vi.version = oggpack_read(opb, 32);
            if (vi.version != 0) return (OV_EVERSION);

            vi.channels = oggpack_read(opb, 8);
            vi.rate = oggpack_read(opb, 32);

            vi.bitrate_upper = (int)oggpack_read(opb, 32);
            vi.bitrate_nominal = (int)oggpack_read(opb, 32);
            vi.bitrate_lower = (int)oggpack_read(opb, 32);

            bs = (int)oggpack_read(opb, 4);
            if (bs < 0) goto err_out;
            ci.blocksizes[0] = 1 << bs;
            bs = (int)oggpack_read(opb, 4);
            if (bs < 0) goto err_out;
            ci.blocksizes[1] = 1 << bs;

            if (vi.rate < 1) goto err_out;
            if (vi.channels < 1) goto err_out;
            if (ci.blocksizes[0] < 64) goto err_out;
            if (ci.blocksizes[1] < ci.blocksizes[0]) goto err_out;
            if (ci.blocksizes[1] > 8192) goto err_out;

            if (oggpack_read(opb, 1) != 1) goto err_out; /* EOP check */

            return (0);
            err_out:
            vorbis_info_clear(vi);
            return (OV_EBADHEADER);
        }

        public static int _vorbis_unpack_comment(vorbis_comment vc, oggpack_buffer opb)
        {
            int i;
            int vendorlen = (int)oggpack_read(opb, 32);
            if (vendorlen < 0) goto err_out;
            if (vendorlen > opb.storage - 8) goto err_out;
            vc.vendor = new ArrayPointer(); //_ogg_calloc(vendorlen + 1, 1);
            _v_readstring(opb, vc.vendor, vendorlen);
            i = (int)oggpack_read(opb, 32);
            if (i < 0) goto err_out;
            if (i > ((opb.storage - oggpack_bytes(opb)) >> 2)) goto err_out;
            vc.comments = i;
            vc.user_comments = new ArrayPointer[i]; //_ogg_calloc(vc.comments + 1, sizeof(vc.user_comments));
            vc.comment_lengths = new long[i]; //_ogg_calloc(vc.comments + 1, sizeof(vc.comment_lengths));

            for (i = 0; i < vc.comments; i++)
            {
                int len = (int)oggpack_read(opb, 32);
                if (len < 0) goto err_out;
                if (len > opb.storage - oggpack_bytes(opb)) goto err_out;
                vc.comment_lengths[i] = len;
                vc.user_comments[i] = new ArrayPointer(); //_ogg_calloc(len + 1, 1);
                _v_readstring(opb, vc.user_comments[i], len);
            }
            if (oggpack_read(opb, 1) != 1) goto err_out; /* EOP check */

            return (0);
            err_out:
            vorbis_comment_clear(vc);
            return (OV_EBADHEADER);
        }

        /* all of the real encoding details are here.  The modes, books,
           everything */
        public static int _vorbis_unpack_books(vorbis_info vi, oggpack_buffer opb)
        {
            codec_setup_info ci = vi.codec_setup;
            int i;

            /* codebooks */
            ci.books = (int)oggpack_read(opb, 8) + 1;
            if (ci.books <= 0) goto err_out;
            for (i = 0; i < ci.books; i++)
            {
                ci.book_param[i] = vorbis_staticbook_unpack(opb);
                if (ci.book_param[i] == null) goto err_out;
            }

            /* time backend settings; hooks are unused */
            {
                int times = (int)oggpack_read(opb, 6) + 1;
                if (times <= 0) goto err_out;
                for (i = 0; i < times; i++)
                {
                    int test = (int)oggpack_read(opb, 16);
                    if (test < 0 || test >= VI_TIMEB) goto err_out;
                }
            }

            /* floor backend settings */
            ci.floors = (int)oggpack_read(opb, 6) + 1;
            if (ci.floors <= 0) goto err_out;
            for (i = 0; i < ci.floors; i++)
            {
                ci.floor_type[i] = (int)oggpack_read(opb, 16);
                if (ci.floor_type[i] < 0 || ci.floor_type[i] >= VI_FLOORB) goto err_out;
                ci.floor_param[i] = _floor_P[ci.floor_type[i]].unpack(vi, opb);
                if (ci.floor_param[i] == null) goto err_out;
            }

            /* residue backend settings */
            ci.residues = (int)oggpack_read(opb, 6) + 1;
            if (ci.residues <= 0) goto err_out;
            for (i = 0; i < ci.residues; i++)
            {
                ci.residue_type[i] = (int)oggpack_read(opb, 16);
                if (ci.residue_type[i] < 0 || ci.residue_type[i] >= VI_RESB) goto err_out;
                ci.residue_param[i] = _residue_P[ci.residue_type[i]].unpack(vi, opb);
                if (ci.residue_param[i] == null) goto err_out;
            }

            /* map backend settings */
            ci.maps = (int)oggpack_read(opb, 6) + 1;
            if (ci.maps <= 0) goto err_out;
            for (i = 0; i < ci.maps; i++)
            {
                ci.map_type[i] = (int)oggpack_read(opb, 16);
                if (ci.map_type[i] < 0 || ci.map_type[i] >= VI_MAPB) goto err_out;
                ci.map_param[i] = _mapping_P[ci.map_type[i]].unpack(vi, opb);
                if (ci.map_param[i] == null) goto err_out;
            }

            /* mode settings */
            ci.modes = (int)oggpack_read(opb, 6) + 1;
            if (ci.modes <= 0) goto err_out;
            for (i = 0; i < ci.modes; i++)
            {
                ci.mode_param[i] = new vorbis_info_mode(); //_ogg_calloc(1, sizeof(*ci.mode_param[i]));
                ci.mode_param[i].blockflag = (int)oggpack_read(opb, 1);
                ci.mode_param[i].windowtype = (int)oggpack_read(opb, 16);
                ci.mode_param[i].transformtype = (int)oggpack_read(opb, 16);
                ci.mode_param[i].mapping = (int)oggpack_read(opb, 8);

                if (ci.mode_param[i].windowtype >= VI_WINDOWB) goto err_out;
                if (ci.mode_param[i].transformtype >= VI_WINDOWB) goto err_out;
                if (ci.mode_param[i].mapping >= ci.maps) goto err_out;
                if (ci.mode_param[i].mapping < 0) goto err_out;
            }

            if (oggpack_read(opb, 1) != 1) goto err_out; /* top level EOP check */

            return (0);
            err_out:
            vorbis_info_clear(vi);
            return (OV_EBADHEADER);
        }

        public static void vorbis_info_clear(vorbis_info vi)
        {
            codec_setup_info ci = vi.codec_setup;
            int i;

            if (ci != null)
            {

                for (i = 0; i < ci.modes; i++)
                    if (ci.mode_param[i] != null) //_ogg_free(ci.mode_param[i]);
                        ci.mode_param[i] = null;

                for (i = 0; i < ci.maps; i++) /* unpack does the range checking */
                    if (ci.map_param[i] != null) /* this may be cleaning up an aborted
                              unpack, in which case the below type
                              cannot be trusted */
                                                 //_mapping_P[ci.map_type[i]].free_info(ci.map_param[i]);
                        ci.map_param[i] = null;

                for (i = 0; i < ci.floors; i++) /* unpack does the range checking */
                    if (ci.floor_param[i] != null) /* this may be cleaning up an aborted
                                unpack, in which case the below type
                                cannot be trusted */
                                                   //_floor_P[ci.floor_type[i]].free_info(ci.floor_param[i]);
                        ci.floor_param[i] = null;

                for (i = 0; i < ci.residues; i++) /* unpack does the range checking */
                    if (ci.residue_param[i] != null) /* this may be cleaning up an aborted
                                  unpack, in which case the below type
                                  cannot be trusted */
                                                     //_residue_P[ci.residue_type[i]].free_info(ci.residue_param[i]);
                        ci.residue_param[i] = null;

                for (i = 0; i < ci.books; i++)
                {
                    if (ci.book_param[i] != null)
                    {
                        /* knows if the book was not alloced */
                        vorbis_staticbook_destroy(ci.book_param[i]);
                    }
                    //if (ci.fullbooks != null)
                    //    vorbis_book_clear(ci.fullbooks + i);
                }
                if (ci.fullbooks != null)
                    //_ogg_free(ci.fullbooks);
                    ci.fullbooks = null;

                for (i = 0; i < ci.psys; i++)
                    //_vi_psy_free(ci.psy_param[i]);
                    ci.psy_param[i] = null;

                //_ogg_free(ci);
                vi.codec_setup = null;
            }

            //memset(vi, 0, sizeof(*vi));
        }

        public static void vorbis_comment_clear(vorbis_comment vc)
        {
            if (vc != null)
            {
                long i;
                if (vc.user_comments != null)
                {
                    for (i = 0; i < vc.comments; i++)
                        if (vc.user_comments[i] != null) vc.user_comments[i] = null; //_ogg_free(vc.user_comments[i]);
                    vc.user_comments = null; //_ogg_free(vc.user_comments);
                }
                if (vc.comment_lengths != null) vc.comment_lengths = null; //_ogg_free(vc.comment_lengths);
                if (vc.vendor != null) vc.vendor = null; //_ogg_free(vc.vendor);
                                                         //memset(vc, 0, sizeof(*vc));
            }
        }

        /* unpacks a codebook from the packet buffer into the codebook struct,
            readies the codebook auxiliary structures for decode *************/
        public static static_codebook vorbis_staticbook_unpack(oggpack_buffer opb)
        {
            long i, j;
            static_codebook s = new static_codebook(); //_ogg_calloc(1, sizeof(*s));
            s.allocedp = 1;

            /* make sure alignment is correct */
            if (oggpack_read(opb, 24) != 0x564342) goto _eofout;

            /* first the basic parameters */
            s.dim = oggpack_read(opb, 16);
            s.entries = oggpack_read(opb, 24);
            if (s.entries == -1) goto _eofout;

            if (ov_ilog((uint)s.dim) + ov_ilog((uint)s.entries) > 24) goto _eofout;

            /* codeword ordering.... length ordered or unordered? */
            switch ((int)oggpack_read(opb, 1))
            {
                case 0:
                    {
                        long unused;
                        /* allocated but unused entries? */
                        unused = oggpack_read(opb, 1);
                        if ((s.entries * (unused != 0 ? 1 : 5) + 7) >> 3 > opb.storage - oggpack_bytes(opb))
                            goto _eofout;
                        /* unordered */
                        s.lengthlist = new long[s.entries]; //_ogg_malloc(sizeof(*s.lengthlist) * s.entries);

                        /* allocated but unused entries? */
                        if (unused != 0)
                        {
                            /* yes, unused entries */

                            for (i = 0; i < s.entries; i++)
                            {
                                if (oggpack_read(opb, 1) != 0)
                                {
                                    long num = oggpack_read(opb, 5);
                                    if (num == -1) goto _eofout;
                                    s.lengthlist[i] = num + 1;
                                }
                                else
                                    s.lengthlist[i] = 0;
                            }
                        }
                        else
                        {
                            /* all entries used; no tagging */
                            for (i = 0; i < s.entries; i++)
                            {
                                long num = oggpack_read(opb, 5);
                                if (num == -1) goto _eofout;
                                s.lengthlist[i] = num + 1;
                            }
                        }

                        break;
                    }
                case 1:
                    /* ordered */
                    {
                        long length = oggpack_read(opb, 5) + 1;
                        if (length == 0) goto _eofout;
                        s.lengthlist = new long[s.entries]; //_ogg_malloc(sizeof(*s.lengthlist) * s.entries);

                        for (i = 0; i < s.entries;)
                        {
                            long num = oggpack_read(opb, ov_ilog((uint)(s.entries - i)));
                            if (num == -1) goto _eofout;
                            if (length > 32 || num > s.entries - i ||
                               (num > 0 && (num - 1) >> (int)(length - 1) > 1))
                            {
                                goto _errout;
                            }
                            if (length > 32) goto _errout;
                            for (j = 0; j < num; j++, i++)
                                s.lengthlist[i] = length;
                            length++;
                        }
                    }
                    break;
                default:
                    /* EOF */
                    goto _eofout;
            }

            /* Do we have a mapping to unpack? */
            switch ((s.maptype = (int)oggpack_read(opb, 4)))
            {
                case 0:
                    /* no mapping */
                    break;
                case 1:
                case 2:
                    /* implicitly populated value mapping */
                    /* explicitly populated value mapping */

                    s.q_min = oggpack_read(opb, 32);
                    s.q_delta = oggpack_read(opb, 32);
                    s.q_quant = (int)oggpack_read(opb, 4) + 1;
                    s.q_sequencep = (int)oggpack_read(opb, 1);
                    if (s.q_sequencep == -1) goto _eofout;

                    {
                        int quantvals = 0;
                        switch (s.maptype)
                        {
                            case 1:
                                quantvals = (int)(s.dim == 0 ? 0 : _book_maptype1_quantvals(s));
                                break;
                            case 2:
                                quantvals = (int)(s.entries * s.dim);
                                break;
                        }

                        /* quantized values */
                        if (((quantvals * s.q_quant + 7) >> 3) > opb.storage - oggpack_bytes(opb))
                            goto _eofout;
                        s.quantlist = new long[quantvals]; //_ogg_malloc(sizeof(*s.quantlist) * quantvals);
                        for (i = 0; i < quantvals; i++)
                            s.quantlist[i] = oggpack_read(opb, s.q_quant);

                        if (quantvals != 0 && s.quantlist[quantvals - 1] == -1) goto _eofout;
                    }
                    break;
                default:
                    goto _errout;
            }

            /* all set */
            return (s);

            _errout:
            _eofout:
            vorbis_staticbook_destroy(s);
            return (null);
        }
    }
}
