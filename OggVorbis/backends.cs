/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2009             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: libvorbis backend and mapping structures; needed for
           static mode headers
  
 Ported by: ETdoFresh

 ********************************************************************/

namespace OggVorbis
{
    public class backends
    {
        /* this would all be simpler/shorter with templates, but.... */
        /* Floor backend generic *****************************************/
        public struct vorbis_func_floor
        {
            public void pack(vorbis_info_floor*, oggpack_buffer*);
            public vorbis_info_floor* (* unpack) (vorbis_info*, oggpack_buffer*);
            public vorbis_look_floor* (* look) (vorbis_dsp_state*, vorbis_info_floor*);
            public void (* free_info) (vorbis_info_floor*);
            public void (* free_look) (vorbis_look_floor*);
            public void* (* inverse1) (vorbis_block vorbis_look_floor*);
            public int (* inverse2) vorbis_block vorbis_look_floor*, void* buffer,float*);
        }

        public struct vorbis_info_floor0
        {
            public int order;
            public long rate;
            public long barkmap;

            public int ampbits;
            public int ampdB;

            public int numbooks; /* <= 16 */
            public int books[16];

            public float lessthan;     /* encode-only config setting hacks for libvorbis */
            public float greaterthan;  /* encode-only config setting hacks for libvorbis */
        }


        public const int VIF_POSIT = 63;
        public const int VIF_CLASS = 16;
        public const int VIF_PARTS = 31;

        public struct vorbis_info_floor1
        {
            public int partitions;                /* 0 to 31 */
            public int partitionclass[VIF_PARTS]; /* 0 to 15 */

            public int class_dim[VIF_CLASS];        /* 1 to 8 */
            public int class_subs[VIF_CLASS];       /* 0,1,2,3 (bits: 1<<n poss) */
            public int class_book[VIF_CLASS];       /* subs ^ dim entries */
            public int class_subbook[VIF_CLASS][8]; /* [VIF_CLASS][subs] */

            public int mult;                      /* 1 2 3 or 4 */
            public int postlist[VIF_POSIT + 2];    /* first two implicit */

            /* encode side analysis parameters */
            public float maxover;
            public float maxunder;
            public float maxerr;

            public float twofitweight;
            public float twofitatten;

            public int n;
        }

        /* Residue backend generic *****************************************/
        public struct vorbis_func_residue
        {
            public void (* pack) (vorbis_info_residue*, oggpack_buffer*);
            public vorbis_info_residue* (* unpack) (vorbis_info*, oggpack_buffer*);
            public vorbis_look_residue* (* look) (vorbis_dsp_state*, vorbis_info_residue*);
            public void (* free_info) (vorbis_info_residue*);
            public void (* free_look) (vorbis_look_residue*);
            public long** (*class)      (struct vorbis_block *,vorbis_look_residue*, int**,int*,int);
            public int (* forward) (oggpack_buffer*,struct vorbis_block *, vorbis_look_residue*, int**,int*,int,long**,int);
            public int (* inverse) (struct vorbis_block *,vorbis_look_residue*, float**,int*,int);
        }

        public struct vorbis_info_residue0
        {
            /* block-partitioned VQ coded straight residue */
            public long begin;
            public long end;

            /* first stage (lossless partitioning) */
            public int grouping;         /* group n vectors per partition */
            public int partitions;       /* possible codebooks for a partition */
            public int partvals;         /* partitions ^ groupbook dim */
            public int groupbook;        /* huffbook for partitioning */
            public int secondstages[64]; /* expanded out to pointers in lookup */
            public int booklist[512];    /* list of second stage books */

            public readonly int classmetric1[64];
            public readonly int classmetric2[64];
        }

        /* Mapping backend generic *****************************************/
        public struct vorbis_func_mapping
        {
            public void (* pack) (vorbis_info*, vorbis_info_mapping*, oggpack_buffer*);
            public vorbis_info_mapping* (* unpack) (vorbis_info*, oggpack_buffer*);
            public void (* free_info) (vorbis_info_mapping*);
            public int (* forward) (struct vorbis_block * vb);
            public int (* inverse) (struct vorbis_block * vb, vorbis_info_mapping*);
        }

        public struct vorbis_info_mapping0
        {
            public int submaps;  /* <= 16 */
            public int chmuxlist[256];   /* up to 256 channels in a Vorbis stream */

            public int floorsubmap[16];   /* [mux] submap to floors */
            public int residuesubmap[16]; /* [mux] submap to residue */

            public int coupling_steps;
            public int coupling_mag[256];
            public int coupling_ang[256];

        }
    }
}