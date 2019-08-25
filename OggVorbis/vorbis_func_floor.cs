using System;

namespace OggVorbis
{
    public class vorbis_func_floor
    {
        public Action<vorbis_info_floor, oggpack_buffer> pack;
        public Func<vorbis_info, oggpack_buffer, vorbis_info_floor> unpack;
        public Func<vorbis_dsp_state, vorbis_info_floor, vorbis_look_floor> look;
        public Action<vorbis_info_floor> free_info;
        public Action<vorbis_look_floor> free_look;
        public Action<vorbis_block, vorbis_look_floor> inverse1;
        public Func<vorbis_block, vorbis_look_floor, ArrayPointer, float[]> inverse2;
    }
}