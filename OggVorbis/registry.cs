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

 function: registry for time, floor, res backends and channel mappings
  
 Ported by: ETdoFresh

 ********************************************************************/

#include "vorbis/codec.h"
#include "codec_internal.h"
#include "registry.h"
#include "misc.h"
/* seems like major overkill now; the backend numbers will grow into
   the infrastructure soon enough */

extern const vorbis_func_floor floor0_exportbundle;
extern const vorbis_func_floor floor1_exportbundle;
extern const vorbis_func_residue residue0_exportbundle;
extern const vorbis_func_residue residue1_exportbundle;
extern const vorbis_func_residue residue2_exportbundle;
extern const vorbis_func_mapping mapping0_exportbundle;

const vorbis_func_floor*const _floor_P[]={
  &floor0_exportbundle,
  &floor1_exportbundle,
};

const vorbis_func_residue*const _residue_P[]={
  &residue0_exportbundle,
  &residue1_exportbundle,
  &residue2_exportbundle,
};

const vorbis_func_mapping*const _mapping_P[]={
  &mapping0_exportbundle,
};

#define VI_TRANSFORMB 1
#define VI_WINDOWB 1
#define VI_TIMEB 1
#define VI_FLOORB 2
#define VI_RESB 3
#define VI_MAPB 1