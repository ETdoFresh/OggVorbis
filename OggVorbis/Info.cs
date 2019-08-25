using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public class Info
    {
        /* used by synthesis, which has a full, alloced vi */
        public static void vorbis_info_init(vorbis_info vi)
        {
            vi.codec_setup = new codec_setup_info();
        }

        public static void vorbis_comment_init(vorbis_comment vc)
        {
        }
    }
}
