using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public class psy
    {
        public static void _vi_psy_free(vorbis_info_psy i)
        {
            if (i != null)
            {
                //memset(i, 0, sizeof(*i));
                //_ogg_free(i);
            }
        }
    }
}
