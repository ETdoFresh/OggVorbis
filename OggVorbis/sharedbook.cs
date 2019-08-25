using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace OggVorbis
{
    public class sharedbook
    {
        public static void vorbis_staticbook_destroy(static_codebook b)
        {
            if (b.allocedp != 0)
            {
                if (b.quantlist != null) b.quantlist = null; //_ogg_free(b.quantlist);
                if (b.lengthlist != null) b.lengthlist = null; // _ogg_free(b.lengthlist);
                //memset(b, 0, sizeof(*b));
                //_ogg_free(b);
            } /* otherwise, it is in static memory */
        }

        public static void vorbis_book_clear(codebook b)
        {
            /* static book is not cleared; we're likely called on the lookup and
               the static codebook belongs to the info struct */
            if (b.valuelist != null) b.valuelist = null; //_ogg_free(b.valuelist);
            if (b.codelist != null) b.codelist = null; //_ogg_free(b.codelist);

            if (b.dec_index != null) b.dec_index = null; //_ogg_free(b.dec_index);
            if (b.dec_codelengths != null) b.dec_codelengths = null; //_ogg_free(b.dec_codelengths);
            if (b.dec_firsttable != null) b.dec_firsttable = null; //_ogg_free(b.dec_firsttable);

            //memset(b, 0, sizeof(*b));
        }

        /**** pack/unpack helpers ******************************************/

        public static int ov_ilog(uint v)
        {
            int ret;
            for (ret = 0; v != 0; ret++) v >>= 1;
            return ret;
        }

        /* there might be a straightforward one-line way to do the below
   that's portable and totally safe against roundoff, but I haven't
   thought of it.  Therefore, we opt on the side of caution */
        public static long _book_maptype1_quantvals(static_codebook b)
        {
            long vals;
            if (b.entries < 1)
            {
                return (0);
            }
            vals = (long)Floor(Pow((float)b.entries, 1f / b.dim));

            /* the above *should* be reliable, but we'll not assume that FP is
               ever reliable when bitstream sync is at stake; verify via integer
               means that vals really is the greatest value of dim for which
               vals^b.bim <= b.entries */
            /* treat the above as an initial guess */
            if (vals < 1)
            {
                vals = 1;
            }
            while (true)
            {
                long acc = 1;
                long acc1 = 1;
                int i;
                for (i = 0; i < b.dim; i++)
                {
                    if (b.entries / vals < acc) break;
                    acc *= vals;
                    if (long.MaxValue / (vals + 1) < acc1) acc1 = long.MaxValue;
                    else acc1 *= vals + 1;
                }
                if (i >= b.dim && acc <= b.entries && acc1 > b.entries)
                {
                    return (vals);
                }
                else
                {
                    if (i < b.dim || acc > b.entries)
                    {
                        vals--;
                    }
                    else
                    {
                        vals++;
                    }
                }
            }
        }
    }
}
