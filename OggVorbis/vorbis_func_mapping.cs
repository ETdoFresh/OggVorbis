using System;

namespace OggVorbis
{
    public class vorbis_func_mapping
    {
        public Action<vorbis_info, vorbis_info_mapping, oggpack_buffer> pack;
        public Func<vorbis_info, oggpack_buffer, vorbis_info_mapping> unpack;
        public Action<vorbis_info_mapping> free_info;
        public Func<vorbis_block, int> forward;
        public Func<vorbis_block, vorbis_info_mapping, int> inverse;
    }
}