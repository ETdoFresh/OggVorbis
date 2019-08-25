using System;

namespace OggVorbis
{
    public class vorbis_func_residue
    {
        public Action<vorbis_info_residue, oggpack_buffer> pack;
        public Func<vorbis_info, oggpack_buffer, vorbis_info_residue> unpack;
        public Func<vorbis_dsp_state, vorbis_info_residue, vorbis_look_residue> look;
        public Action<vorbis_info_residue> free_info;
        public Action<vorbis_look_residue> free_look;
        public Func<vorbis_block, vorbis_look_residue, int[][], int[], int[], long> @class;
        public Func<oggpack_buffer, vorbis_block, vorbis_look_residue, int[][], int[], int, long[][], int, int> forward;
        public Func<vorbis_block, vorbis_look_residue, float[][], int[], int, int> inverse;
    }
}