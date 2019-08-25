using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OggVorbis
{
    public class registry
    {
        public static int VI_TRANSFORMB = 1;
        public static int VI_WINDOWB = 1;
        public static int VI_TIMEB = 1;
        public static int VI_FLOORB = 2;
        public static int VI_RESB = 3;
        public static int VI_MAPB = 1;

        /* seems like major overkill now; the backend numbers will grow into
           the infrastructure soon enough */

        public static readonly vorbis_func_floor[] _floor_P ={
  //&floor0_exportbundle,
  //&floor1_exportbundle,
};

        public static readonly vorbis_func_residue[] _residue_P ={
  //&residue0_exportbundle,
  //&residue1_exportbundle,
  //&residue2_exportbundle,
};

        public static readonly vorbis_func_mapping[] _mapping_P ={
  //&mapping0_exportbundle,
};

    }
}
