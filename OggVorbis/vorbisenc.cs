/********************************************************************
 *                                                                  *
 * THIS FILE IS PART OF THE OggVorbis SOFTWARE CODEC SOURCE CODE.   *
 * USE, DISTRIBUTION AND REPRODUCTION OF THIS LIBRARY SOURCE IS     *
 * GOVERNED BY A BSD-STYLE SOURCE LICENSE INCLUDED WITH THIS SOURCE *
 * IN 'COPYING'. PLEASE READ THESE TERMS BEFORE DISTRIBUTING.       *
 *                                                                  *
 * THE OggVorbis SOURCE CODE IS (C) COPYRIGHT 1994-2001             *
 * by the Xiph.Org Foundation http://www.xiph.org/                  *
 *                                                                  *
 ********************************************************************

 function: vorbis encode-engine setup
  
 Ported by: ETdoFresh

 ********************************************************************/

/** \file
 * Libvorbisenc is a convenient API for setting up an encoding
 * environment using libvorbis. Libvorbisenc encapsulates the
 * actions needed to set up the encoder properly.
 */
namespace OggVorbis
{
    public class vorbisenc
    {
        /* careful with this; it's using static array sizing to make managing
           all the modes a little less annoying.  If we use a residue backend
           with > 12 partition types, or a different division of iteration,
           this needs to be updated. */
        struct static_bookblock
        {
            public static_codebook[][] books;
        }

        struct vorbis_residue_template
        {
            public int res_type;
            public int limit_type; /* 0 lowpass limited, 1 point stereo limited */
            public int grouping;
            public vorbis_info_residue0[] res;
            public static_codebook[] book_aux;
            public static_codebook[] book_aux_managed;
            public static_bookblock[] books_base;
            public static_bookblock[] books_base_managed;
        }

        struct vorbis_mapping_template
        {
            public vorbis_info_mapping0[] map;
            public vorbis_residue_template[] res;
        }

        struct vp_adjblock
        {
            public int[] block;
        }

        struct compandblock
        {
            int[] data;
        }

        /* high level configuration information for setting things up
   step-by-step with the detailed vorbis_encode_ctl interface.
   There's a fair amount of redundancy such that interactive setup
   does not directly deal with any vorbis_info or codec_setup_info
   initialization; it's all stored (until full init) in this highlevel
   setup, then flushed out to the real codec setup structs later. */

        struct att3
        {
            int[] att;
            float boost;
            float decay;
        }

        struct adj3
        {
            int[] data;
        }

        struct adj_stereo
        {
            int[] pre;
            int[] post;
            float[] kHz;
            float[] lowpasskHz;
        }

        struct noiseguard
        {
            int lo;
            int hi;
            int @fixed;
        }

        struct noise3
        {
            int[][] data;
        }

        struct ve_setup_data_template
        {
            int mappings;
            double[] rate_mapping;
            double[] quality_mapping;
            int coupling_restriction;
            long samplerate_min_restriction;
            long samplerate_max_restriction;


            int[] blocksize_short;
            int[] blocksize_long;

            att3[] psy_tone_masteratt;
            int[] psy_tone_0dB;
            int[] psy_tone_dBsuppress;

            vp_adjblock[] psy_tone_adj_impulse;
            vp_adjblock[] psy_tone_adj_long;
            vp_adjblock[] psy_tone_adj_other;

            noiseguard[] psy_noiseguards;
            noise3[] psy_noise_bias_impulse;
            noise3[] psy_noise_bias_padding;
            noise3[] psy_noise_bias_trans;
            noise3[] psy_noise_bias_long;
            int[] psy_noise_dBsuppress;

            compandblock[] psy_noise_compand;
            double[] psy_noise_compand_short_mapping;
            double[] psy_noise_compand_long_mapping;

            int[] psy_noise_normal_start;
            int[] psy_noise_normal_partition;
            double[] psy_noise_normal_thresh;

            int[] psy_ath_float;
            int[] psy_ath_abs;

            double[] psy_lowpass;

            vorbis_info_psy_global[] global_params;
            double[] global_mapping;
            adj_stereo[] stereo_modes;

            static_codebook[] floor_books;
            vorbis_info_floor1[] floor_params;
            int floor_mappings;
            int[][] floor_mapping_list;

            vorbis_mapping_template[] maps;
        }

        /* a few static coder conventions */
        public static readonly vorbis_info_mode[] _mode_template ={
  {0,0,0,0},
  {1,0,0,1}
};

        public static readonly vorbis_info_mapping0[] _map_nominal ={
  {1, {0,0}, {0}, {0}, 1,{0},{1}},
  {1, {0,0}, {1}, {1}, 1,{0},{1}}
};





        static const ve_setup_data_template*const setup_list[]={
  &ve_setup_44_stereo,
  &ve_setup_44_51,
  &ve_setup_44_uncoupled,

  &ve_setup_32_stereo,
  &ve_setup_32_uncoupled,

  &ve_setup_22_stereo,
  &ve_setup_22_uncoupled,
  &ve_setup_16_stereo,
  &ve_setup_16_uncoupled,

  &ve_setup_11_stereo,
  &ve_setup_11_uncoupled,
  &ve_setup_8_stereo,
  &ve_setup_8_uncoupled,

  &ve_setup_X_stereo,
  &ve_setup_X_uncoupled,
  &ve_setup_XX_stereo,
  &ve_setup_XX_uncoupled,
  0
};

        static void vorbis_encode_floor_setup(vorbis_info* vi, int s,
                                     const static_codebook*const *const *const books,
                                     const vorbis_info_floor1*in,
                                     const int* x){
  int i, k,is=s;
  vorbis_info_floor1* f = _ogg_calloc(1, sizeof(*f));
        codec_setup_info* ci = vi->codec_setup;

        memcpy(f, in+x[is],sizeof(* f));

  /* books */
  {
    int partitions = f->partitions;
        int maxclass = -1;
        int maxbook = -1;
    for(i=0;i<partitions;i++)
      if(f->partitionclass[i]>maxclass)maxclass=f->partitionclass[i];
    for(i=0;i<=maxclass;i++){
      if(f->class_book[i]>maxbook)maxbook=f->class_book[i];
      f->class_book[i]+=ci->books;
      for(k=0;k<(1<<f->class_subs[i]);k++){
        if(f->class_subbook[i][k]>maxbook)maxbook=f->class_subbook[i][k];
        if(f->class_subbook[i][k]>=0)f->class_subbook[i][k]+=ci->books;
      }
}

    for(i=0;i<=maxbook;i++)
      ci->book_param[ci->books++]=(static_codebook*) books[x[is]][i];
  }

  /* for now, we're only using floor 1 */
  ci->floor_type[ci->floors]=1;
  ci->floor_param[ci->floors]=f;
  ci->floors++;

  return;
}

static void vorbis_encode_global_psych_setup(vorbis_info* vi, double s,
                                            const vorbis_info_psy_global*in,
                                            const double* x)
{
    int i,is= s;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy_global* g = &ci->psy_g_param;

    memcpy(g, in +(int)x[is], sizeof(*g));

    ds = x[is] * (1.- ds) + x[is +1] * ds;
  is= (int)ds;
    ds -=is;
    if (ds == 0 && is> 0)
    {
    is --;
        ds = 1.;
    }

    /* interpolate the trigger threshholds */
    for (i = 0; i < 4; i++)
    {
        g->preecho_thresh[i] =in[is].preecho_thresh[i]* (1.-ds)+in[is+1].preecho_thresh[i]* ds;
g->postecho_thresh[i]=in[is].postecho_thresh[i]* (1.-ds)+in[is+1].postecho_thresh[i]* ds;
  }
  g->ampmax_att_per_sec=ci->hi.amplitude_track_dBpersec;
  return;
}

static void vorbis_encode_global_stereo(vorbis_info* vi,
                                        const highlevel_encode_setup*const hi,
                                        const adj_stereo* p)
{
    float s = hi->stereo_point_setting;
    int i,is= s;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy_global* g = &ci->psy_g_param;

    if (p)
    {
        memcpy(g->coupling_prepointamp, p[is].pre, sizeof(*p[is].pre) * PACKETBLOBS);
        memcpy(g->coupling_postpointamp, p[is].post, sizeof(*p[is].post) * PACKETBLOBS);

        if (hi->managed)
        {
            /* interpolate the kHz threshholds */
            for (i = 0; i < PACKETBLOBS; i++)
            {
                float kHz = p[is].kHz[i] * (1.- ds) + p[is +1].kHz[i] * ds;
                g->coupling_pointlimit[0][i] = kHz * 1000./ vi->rate * ci->blocksizes[0];
                g->coupling_pointlimit[1][i] = kHz * 1000./ vi->rate * ci->blocksizes[1];
                g->coupling_pkHz[i] = kHz;

                kHz = p[is].lowpasskHz[i] * (1.- ds) + p[is +1].lowpasskHz[i] * ds;
                g->sliding_lowpass[0][i] = kHz * 1000./ vi->rate * ci->blocksizes[0];
                g->sliding_lowpass[1][i] = kHz * 1000./ vi->rate * ci->blocksizes[1];

            }
        }
        else
        {
            float kHz = p[is].kHz[PACKETBLOBS / 2] * (1.- ds) + p[is +1].kHz[PACKETBLOBS / 2] * ds;
            for (i = 0; i < PACKETBLOBS; i++)
            {
                g->coupling_pointlimit[0][i] = kHz * 1000./ vi->rate * ci->blocksizes[0];
                g->coupling_pointlimit[1][i] = kHz * 1000./ vi->rate * ci->blocksizes[1];
                g->coupling_pkHz[i] = kHz;
            }

            kHz = p[is].lowpasskHz[PACKETBLOBS / 2] * (1.- ds) + p[is +1].lowpasskHz[PACKETBLOBS / 2] * ds;
            for (i = 0; i < PACKETBLOBS; i++)
            {
                g->sliding_lowpass[0][i] = kHz * 1000./ vi->rate * ci->blocksizes[0];
                g->sliding_lowpass[1][i] = kHz * 1000./ vi->rate * ci->blocksizes[1];
            }
        }
    }
    else
    {
        for (i = 0; i < PACKETBLOBS; i++)
        {
            g->sliding_lowpass[0][i] = ci->blocksizes[0];
            g->sliding_lowpass[1][i] = ci->blocksizes[1];
        }
    }
    return;
}

static void vorbis_encode_psyset_setup(vorbis_info* vi, double s,
                                       const int* nn_start,
                                       const int* nn_partition,
                                       const double* nn_thresh,
                                       int block)
{
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];
    highlevel_encode_setup* hi = &ci->hi;
    int is= s;

    if (block >= ci->psys)
        ci->psys = block + 1;
    if (!p)
    {
        p = _ogg_calloc(1, sizeof(*p));
        ci->psy_param[block] = p;
    }

    memcpy(p, &_psy_info_template, sizeof(*p));
    p->blockflag = block >> 1;

    if (hi->noise_normalize_p)
    {
        p->normal_p = 1;
        p->normal_start = nn_start[is];
        p->normal_partition = nn_partition[is];
        p->normal_thresh = nn_thresh[is];
    }

    return;
}

static void vorbis_encode_tonemask_setup(vorbis_info* vi, double s, int block,
                                         const att3* att,
                                         const int* max,
                                         const vp_adjblock*in)
{
    int i,is= s;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];

    /* 0 and 2 are only used by bitmanagement, but there's no harm to always
       filling the values in here */
    p->tone_masteratt[0] = att[is].att[0] * (1.- ds) + att[is +1].att[0] * ds;
    p->tone_masteratt[1] = att[is].att[1] * (1.- ds) + att[is +1].att[1] * ds;
    p->tone_masteratt[2] = att[is].att[2] * (1.- ds) + att[is +1].att[2] * ds;
    p->tone_centerboost = att[is].boost * (1.- ds) + att[is +1].boost * ds;
    p->tone_decay = att[is].decay * (1.- ds) + att[is +1].decay * ds;

    p->max_curve_dB = max[is] * (1.- ds) + max[is +1] * ds;

    for (i = 0; i < P_BANDS; i++)
        p->toneatt[i] =in[is].block[i]* (1.-ds)+in[is+1].block[i]* ds;
  return;
}


static void vorbis_encode_compand_setup(vorbis_info* vi, double s, int block,
                                        const compandblock*in,
                                        const double* x)
{
    int i,is= s;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];

    ds = x[is] * (1.- ds) + x[is +1] * ds;
  is= (int)ds;
    ds -=is;
    if (ds == 0 && is> 0)
    {
    is --;
        ds = 1.;
    }

    /* interpolate the compander settings */
    for (i = 0; i < NOISE_COMPAND_LEVELS; i++)
        p->noisecompand[i] =in[is].data[i]* (1.-ds)+in[is+1].data[i]* ds;
  return;
}

static void vorbis_encode_peak_setup(vorbis_info* vi, double s, int block,
                                    const int* suppress)
{
    int is= s;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];

    p->tone_abs_limit = suppress[is] * (1.- ds) + suppress[is +1] * ds;

    return;
}

static void vorbis_encode_noisebias_setup(vorbis_info* vi, double s, int block,
                                         const int* suppress,
                                         const noise3*in,
                                         const noiseguard* guard,
                                         double userbias)
{
    int i,is= s,j;
    double ds = s -is;
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];

    p->noisemaxsupp = suppress[is] * (1.- ds) + suppress[is +1] * ds;
    p->noisewindowlomin = guard[block].lo;
    p->noisewindowhimin = guard[block].hi;
    p->noisewindowfixed = guard[block].fixed;

    for (j = 0; j < P_NOISECURVES; j++)
        for (i = 0; i < P_BANDS; i++)
            p->noiseoff[j][i] =in[is].data[j][i]* (1.-ds)+in[is+1].data[j][i]* ds;

  /* impulse blocks may take a user specified bias to boost the
     nominal/high noise encoding depth */
  for(j=0;j<P_NOISECURVES;j++){
    float min = p->noiseoff[j][0] + 6; /* the lowest it can go */
    for(i=0;i<P_BANDS;i++){
      p->noiseoff[j][i]+=userbias;
      if(p->noiseoff[j][i]<min)p->noiseoff[j][i]=min;
    }
  }

  return;
}

static void vorbis_encode_ath_setup(vorbis_info* vi, int block)
{
    codec_setup_info* ci = vi->codec_setup;
    vorbis_info_psy* p = ci->psy_param[block];

    p->ath_adjatt = ci->hi.ath_floating_dB;
    p->ath_maxatt = ci->hi.ath_absolute_dB;
    return;
}


static int book_dup_or_new(codec_setup_info* ci,const static_codebook* book)
{
    int i;
    for (i = 0; i < ci->books; i++)
        if (ci->book_param[i] == book) return (i);

    return (ci->books++);
}

static void vorbis_encode_blocksize_setup(vorbis_info* vi, double s,
                                         const int* shortb,const int* longb)
{

    codec_setup_info* ci = vi->codec_setup;
    int is= s;

    int blockshort = shortb[is];
    int blocklong = longb[is];
    ci->blocksizes[0] = blockshort;
    ci->blocksizes[1] = blocklong;

}

static void vorbis_encode_residue_setup(vorbis_info* vi,
                                        int number, int block,
                                        const vorbis_residue_template* res)
{

    codec_setup_info* ci = vi->codec_setup;
    int i;

    vorbis_info_residue0* r = ci->residue_param[number] =
      _ogg_malloc(sizeof(*r));

    memcpy(r, res->res, sizeof(*r));
    if (ci->residues <= number) ci->residues = number + 1;

    r->grouping = res->grouping;
    ci->residue_type[number] = res->res_type;

    /* fill in all the books */
    {
        int booklist = 0, k;

        if (ci->hi.managed)
        {
            for (i = 0; i < r->partitions; i++)
                for (k = 0; k < 4; k++)
                    if (res->books_base_managed->books[i][k])
                        r->secondstages[i] |= (1 << k);

            r->groupbook = book_dup_or_new(ci, res->book_aux_managed);
            ci->book_param[r->groupbook] = (static_codebook*)res->book_aux_managed;

            for (i = 0; i < r->partitions; i++)
            {
                for (k = 0; k < 4; k++)
                {
                    if (res->books_base_managed->books[i][k])
                    {
                        int bookid = book_dup_or_new(ci, res->books_base_managed->books[i][k]);
                        r->booklist[booklist++] = bookid;
                        ci->book_param[bookid] = (static_codebook*)res->books_base_managed->books[i][k];
                    }
                }
            }

        }
        else
        {

            for (i = 0; i < r->partitions; i++)
                for (k = 0; k < 4; k++)
                    if (res->books_base->books[i][k])
                        r->secondstages[i] |= (1 << k);

            r->groupbook = book_dup_or_new(ci, res->book_aux);
            ci->book_param[r->groupbook] = (static_codebook*)res->book_aux;

            for (i = 0; i < r->partitions; i++)
            {
                for (k = 0; k < 4; k++)
                {
                    if (res->books_base->books[i][k])
                    {
                        int bookid = book_dup_or_new(ci, res->books_base->books[i][k]);
                        r->booklist[booklist++] = bookid;
                        ci->book_param[bookid] = (static_codebook*)res->books_base->books[i][k];
                    }
                }
            }
        }
    }

    /* lowpass setup/pointlimit */
    {
        double freq = ci->hi.lowpass_kHz * 1000.;
        vorbis_info_floor1* f = ci->floor_param[block]; /* by convention */
        double nyq = vi->rate / 2.;
        long blocksize = ci->blocksizes[block] >> 1;

        /* lowpass needs to be set in the floor and the residue. */
        if (freq > nyq) freq = nyq;
        /* in the floor, the granularity can be very fine; it doesn't alter
           the encoding structure, only the samples used to fit the floor
           approximation */
        f->n = freq / nyq * blocksize;

        /* this res may by limited by the maximum pointlimit of the mode,
           not the lowpass. the floor is always lowpass limited. */
        switch (res->limit_type)
        {
            case 1: /* point stereo limited */
                if (ci->hi.managed)
                    freq = ci->psy_g_param.coupling_pkHz[PACKETBLOBS - 1] * 1000.;
                else
                    freq = ci->psy_g_param.coupling_pkHz[PACKETBLOBS / 2] * 1000.;
                if (freq > nyq) freq = nyq;
                break;
            case 2: /* LFE channel; lowpass at ~ 250Hz */
                freq = 250;
                break;
            default:
                /* already set */
                break;
        }

        /* in the residue, we're constrained, physically, by partition
           boundaries.  We still lowpass 'wherever', but we have to round up
           here to next boundary, or the vorbis spec will round it *down* to
           previous boundary in encode/decode */
        if (ci->residue_type[number] == 2)
        {
            /* residue 2 bundles together multiple channels; used by stereo
               and surround.  Count the channels in use */
            /* Multiple maps/submaps can point to the same residue.  In the case
               of residue 2, they all better have the same number of
               channels/samples. */
            int j, k, ch = 0;
            for (i = 0; i < ci->maps && ch == 0; i++)
            {
                vorbis_info_mapping0* mi = (vorbis_info_mapping0*)ci->map_param[i];
                for (j = 0; j < mi->submaps && ch == 0; j++)
                    if (mi->residuesubmap[j] == number) /* we found a submap referencing theis residue backend */
                        for (k = 0; k < vi->channels; k++)
                            if (mi->chmuxlist[k] == j) /* this channel belongs to the submap */
                                ch++;
            }

            r->end = (int)((freq / nyq * blocksize * ch) / r->grouping + .9) * /* round up only if we're well past */
              r->grouping;
            /* the blocksize and grouping may disagree at the end */
            if (r->end > blocksize * ch) r->end = blocksize * ch / r->grouping * r->grouping;

        }
        else
        {

            r->end = (int)((freq / nyq * blocksize) / r->grouping + .9) * /* round up only if we're well past */
              r->grouping;
            /* the blocksize and grouping may disagree at the end */
            if (r->end > blocksize) r->end = blocksize / r->grouping * r->grouping;

        }

        if (r->end == 0) r->end = r->grouping; /* LFE channel */

    }
}

/* we assume two maps in this encoder */
static void vorbis_encode_map_n_res_setup(vorbis_info* vi, double s,
                                          const vorbis_mapping_template* maps)
{

    codec_setup_info* ci = vi->codec_setup;
    int i, j,is= s,modes = 2;
    const vorbis_info_mapping0* map = maps[is].map;
    const vorbis_info_mode* mode = _mode_template;
    const vorbis_residue_template* res = maps[is].res;

    if (ci->blocksizes[0] == ci->blocksizes[1]) modes = 1;

    for (i = 0; i < modes; i++)
    {

        ci->map_param[i] = _ogg_calloc(1, sizeof(*map));
    ci->mode_param[i] = _ogg_calloc(1, sizeof(*mode));

    memcpy(ci->mode_param[i], mode + i, sizeof(*_mode_template));
    if (i >= ci->modes) ci->modes = i + 1;

    ci->map_type[i] = 0;
    memcpy(ci->map_param[i], map + i, sizeof(*map));
    if (i >= ci->maps) ci->maps = i + 1;

    for (j = 0; j < map[i].submaps; j++)
        vorbis_encode_residue_setup(vi, map[i].residuesubmap[j], i
                                    , res + map[i].residuesubmap[j]);
}
}

static double setting_to_approx_bitrate(vorbis_info* vi)
{
    codec_setup_info* ci = vi->codec_setup;
    highlevel_encode_setup* hi = &ci->hi;
    ve_setup_data_template* setup = (ve_setup_data_template*)hi->setup;
    int is= hi->base_setting;
    double ds = hi->base_setting -is;
    int ch = vi->channels;
    const double* r = setup->rate_mapping;

    if (r == NULL)
        return (-1);

    return ((r[is] * (1.- ds) + r[is +1] * ds) * ch);
}

static const void* get_setup_template(long ch, long srate,
                                      double req, int q_or_bitrate,
                                      double * base_setting){
  int i = 0, j;
  if(q_or_bitrate) req/=ch;

  while(setup_list[i]){
    if(setup_list[i]->coupling_restriction==-1 ||
       setup_list[i]->coupling_restriction==ch){
      if(srate>=setup_list[i]->samplerate_min_restriction &&
         srate<=setup_list[i]->samplerate_max_restriction){
        int mappings = setup_list[i]->mappings;
const double* map = (q_or_bitrate ?
             setup_list[i]->rate_mapping :
             setup_list[i]->quality_mapping);

        /* the template matches.  Does the requested quality mode
           fall within this template's modes? */
        if(req<map[0]){++i;continue;}
        if(req>map[setup_list[i]->mappings]){++i;continue;}
        for(j=0;j<mappings;j++)
          if(req>=map[j] && req<map[j + 1])break;
        /* an all-points match */
        if(j==mappings)
          * base_setting = j - .001;
        else{
          float low = map[j];
float high = map[j + 1];
float del = (req - low) / (high - low);
          * base_setting = j + del;
        }

        return(setup_list[i]);
      }
    }
    i++;
  }

  return NULL;
}

/* encoders will need to use vorbis_info_init beforehand and call
   vorbis_info clear when all done */

/* two interfaces; this, more detailed one, and later a convenience
   layer on top */

/* the final setup call */

/**
 * This function performs the last stage of three-step encoding setup, as
 * described in the API overview under managed bitrate modes.
 *
 * Before this function is called, the \ref vorbis_info struct should be
 * initialized by using vorbis_info_init() from the libvorbis API, one of
 * \ref vorbis_encode_setup_managed() or \ref vorbis_encode_setup_vbr() called to
 * initialize the high-level encoding setup, and \ref vorbis_encode_ctl()
 * called if necessary to make encoding setup changes.
 * vorbis_encode_setup_init() finalizes the highlevel encoding structure into
 * a complete encoding setup after which the application may make no further
 * setup changes.
 *
 * After encoding, vorbis_info_clear() should be called.
 *
 * \param vi Pointer to an initialized \ref vorbis_info struct.
 *
 * \return Zero for success, and negative values for failure.
 *
 * \retval  0           Success.
 * \retval  OV_EFAULT  Internal logic fault; indicates a bug or heap/stack corruption.
 *
 * \retval OV_EINVAL   Attempt to use vorbis_encode_setup_init() without first
 * calling one of vorbis_encode_setup_managed() or vorbis_encode_setup_vbr() to
 * initialize the high-level encoding setup
 *
 */
int vorbis_encode_setup_init(vorbis_info* vi)
{
    int i, i0 = 0, singleblock = 0;
    codec_setup_info* ci = vi->codec_setup;
    ve_setup_data_template* setup = NULL;
    highlevel_encode_setup* hi = &ci->hi;

    if (ci == NULL) return (OV_EINVAL);
    if (!hi->impulse_block_p) i0 = 1;

    /* too low/high an ATH floater is nonsensical, but doesn't break anything */
    if (hi->ath_floating_dB > -80) hi->ath_floating_dB = -80;
    if (hi->ath_floating_dB < -200) hi->ath_floating_dB = -200;

    /* again, bound this to avoid the app shooting itself int he foot
       too badly */
    if (hi->amplitude_track_dBpersec > 0.) hi->amplitude_track_dBpersec = 0.;
    if (hi->amplitude_track_dBpersec < -99999.) hi->amplitude_track_dBpersec = -99999.;

    /* get the appropriate setup template; matches the fetch in previous
       stages */
    setup = (ve_setup_data_template*)hi->setup;
    if (setup == NULL) return (OV_EINVAL);

    hi->set_in_stone = 1;
    /* choose block sizes from configured sizes as well as paying
       attention to long_block_p and short_block_p.  If the configured
       short and long blocks are the same length, we set long_block_p
       and unset short_block_p */
    vorbis_encode_blocksize_setup(vi, hi->base_setting,
                                  setup->blocksize_short,
                                  setup->blocksize_long);
    if (ci->blocksizes[0] == ci->blocksizes[1]) singleblock = 1;

    /* floor setup; choose proper floor params.  Allocated on the floor
       stack in order; if we alloc only a single long floor, it's 0 */
    for (i = 0; i < setup->floor_mappings; i++)
        vorbis_encode_floor_setup(vi, hi->base_setting,
                                  setup->floor_books,
                                  setup->floor_params,
                                  setup->floor_mapping_list[i]);

    /* setup of [mostly] short block detection and stereo*/
    vorbis_encode_global_psych_setup(vi, hi->trigger_setting,
                                     setup->global_params,
                                     setup->global_mapping);
    vorbis_encode_global_stereo(vi, hi, setup->stereo_modes);

    /* basic psych setup and noise normalization */
    vorbis_encode_psyset_setup(vi, hi->base_setting,
                               setup->psy_noise_normal_start[0],
                               setup->psy_noise_normal_partition[0],
                               setup->psy_noise_normal_thresh,
                               0);
    vorbis_encode_psyset_setup(vi, hi->base_setting,
                               setup->psy_noise_normal_start[0],
                               setup->psy_noise_normal_partition[0],
                               setup->psy_noise_normal_thresh,
                               1);
    if (!singleblock)
    {
        vorbis_encode_psyset_setup(vi, hi->base_setting,
                                   setup->psy_noise_normal_start[1],
                                   setup->psy_noise_normal_partition[1],
                                        setup->psy_noise_normal_thresh,
                                   2);
        vorbis_encode_psyset_setup(vi, hi->base_setting,
                                   setup->psy_noise_normal_start[1],
                                   setup->psy_noise_normal_partition[1],
                                   setup->psy_noise_normal_thresh,
                                   3);
    }

    /* tone masking setup */
    vorbis_encode_tonemask_setup(vi, hi->block[i0].tone_mask_setting, 0,
                                 setup->psy_tone_masteratt,
                                 setup->psy_tone_0dB,
                                 setup->psy_tone_adj_impulse);
    vorbis_encode_tonemask_setup(vi, hi->block[1].tone_mask_setting, 1,
                                 setup->psy_tone_masteratt,
                                 setup->psy_tone_0dB,
                                 setup->psy_tone_adj_other);
    if (!singleblock)
    {
        vorbis_encode_tonemask_setup(vi, hi->block[2].tone_mask_setting, 2,
                                     setup->psy_tone_masteratt,
                                     setup->psy_tone_0dB,
                                     setup->psy_tone_adj_other);
        vorbis_encode_tonemask_setup(vi, hi->block[3].tone_mask_setting, 3,
                                     setup->psy_tone_masteratt,
                                     setup->psy_tone_0dB,
                                     setup->psy_tone_adj_long);
    }

    /* noise companding setup */
    vorbis_encode_compand_setup(vi, hi->block[i0].noise_compand_setting, 0,
                                setup->psy_noise_compand,
                                setup->psy_noise_compand_short_mapping);
    vorbis_encode_compand_setup(vi, hi->block[1].noise_compand_setting, 1,
                                setup->psy_noise_compand,
                                setup->psy_noise_compand_short_mapping);
    if (!singleblock)
    {
        vorbis_encode_compand_setup(vi, hi->block[2].noise_compand_setting, 2,
                                    setup->psy_noise_compand,
                                    setup->psy_noise_compand_long_mapping);
        vorbis_encode_compand_setup(vi, hi->block[3].noise_compand_setting, 3,
                                    setup->psy_noise_compand,
                                    setup->psy_noise_compand_long_mapping);
    }

    /* peak guarding setup  */
    vorbis_encode_peak_setup(vi, hi->block[i0].tone_peaklimit_setting, 0,
                             setup->psy_tone_dBsuppress);
    vorbis_encode_peak_setup(vi, hi->block[1].tone_peaklimit_setting, 1,
                             setup->psy_tone_dBsuppress);
    if (!singleblock)
    {
        vorbis_encode_peak_setup(vi, hi->block[2].tone_peaklimit_setting, 2,
                                 setup->psy_tone_dBsuppress);
        vorbis_encode_peak_setup(vi, hi->block[3].tone_peaklimit_setting, 3,
                                 setup->psy_tone_dBsuppress);
    }

    /* noise bias setup */
    vorbis_encode_noisebias_setup(vi, hi->block[i0].noise_bias_setting, 0,
                                  setup->psy_noise_dBsuppress,
                                  setup->psy_noise_bias_impulse,
                                  setup->psy_noiseguards,
                                  (i0 == 0 ? hi->impulse_noisetune : 0.));
    vorbis_encode_noisebias_setup(vi, hi->block[1].noise_bias_setting, 1,
                                  setup->psy_noise_dBsuppress,
                                  setup->psy_noise_bias_padding,
                                  setup->psy_noiseguards, 0.);
    if (!singleblock)
    {
        vorbis_encode_noisebias_setup(vi, hi->block[2].noise_bias_setting, 2,
                                      setup->psy_noise_dBsuppress,
                                      setup->psy_noise_bias_trans,
                                      setup->psy_noiseguards, 0.);
        vorbis_encode_noisebias_setup(vi, hi->block[3].noise_bias_setting, 3,
                                      setup->psy_noise_dBsuppress,
                                      setup->psy_noise_bias_long,
                                      setup->psy_noiseguards, 0.);
    }

    vorbis_encode_ath_setup(vi, 0);
    vorbis_encode_ath_setup(vi, 1);
    if (!singleblock)
    {
        vorbis_encode_ath_setup(vi, 2);
        vorbis_encode_ath_setup(vi, 3);
    }

    vorbis_encode_map_n_res_setup(vi, hi->base_setting, setup->maps);

    /* set bitrate readonlies and management */
    if (hi->bitrate_av > 0)
        vi->bitrate_nominal = hi->bitrate_av;
    else
    {
        vi->bitrate_nominal = setting_to_approx_bitrate(vi);
    }

    vi->bitrate_lower = hi->bitrate_min;
    vi->bitrate_upper = hi->bitrate_max;
    if (hi->bitrate_av)
        vi->bitrate_window = (double)hi->bitrate_reservoir / hi->bitrate_av;
    else
        vi->bitrate_window = 0.;

    if (hi->managed)
    {
        ci->bi.avg_rate = hi->bitrate_av;
        ci->bi.min_rate = hi->bitrate_min;
        ci->bi.max_rate = hi->bitrate_max;

        ci->bi.reservoir_bits = hi->bitrate_reservoir;
        ci->bi.reservoir_bias =
          hi->bitrate_reservoir_bias;

        ci->bi.slew_damp = hi->bitrate_av_damp;

    }

    return (0);

}

static void vorbis_encode_setup_setting(vorbis_info* vi,
                                       long channels,
                                       long rate)
{
    int i,is;
    codec_setup_info* ci = vi->codec_setup;
    highlevel_encode_setup* hi = &ci->hi;
    const ve_setup_data_template* setup = hi->setup;
    double ds;

    vi->version = 0;
    vi->channels = channels;
    vi->rate = rate;

    hi->impulse_block_p = 1;
    hi->noise_normalize_p = 1;

  is= hi->base_setting;
    ds = hi->base_setting -is;

    hi->stereo_point_setting = hi->base_setting;

    if (!hi->lowpass_altered)
        hi->lowpass_kHz =
          setup->psy_lowpass[is] * (1.- ds) + setup->psy_lowpass[is +1] * ds;

    hi->ath_floating_dB = setup->psy_ath_float[is] * (1.- ds) +
      setup->psy_ath_float[is +1] * ds;
    hi->ath_absolute_dB = setup->psy_ath_abs[is] * (1.- ds) +
      setup->psy_ath_abs[is +1] * ds;

    hi->amplitude_track_dBpersec = -6.;
    hi->trigger_setting = hi->base_setting;

    for (i = 0; i < 4; i++)
    {
        hi->block[i].tone_mask_setting = hi->base_setting;
        hi->block[i].tone_peaklimit_setting = hi->base_setting;
        hi->block[i].noise_bias_setting = hi->base_setting;
        hi->block[i].noise_compand_setting = hi->base_setting;
    }
}

/**
 * This function performs step-one of a three-step variable bitrate
 * (quality-based) encode setup.  It functions similarly to the one-step setup
 * performed by \ref vorbis_encode_init_vbr() but allows an application to
 * make further encode setup tweaks using \ref vorbis_encode_ctl() before
 * finally calling \ref vorbis_encode_setup_init to complete the setup
 * process.
 *
 * Before this function is called, the \ref vorbis_info struct should be
 * initialized by using \ref vorbis_info_init() from the libvorbis API.  After
 * encoding, vorbis_info_clear() should be called.
 *
 * \param vi        Pointer to an initialized vorbis_info struct.
 * \param channels  The number of channels to be encoded.
 * \param rate      The sampling rate of the source audio.
 * \param quality   Desired quality level, currently from -0.1 to 1.0 (lo to hi).
 *
 * \return Zero for success, and negative values for failure.
 *
 * \retval  0          Success
 * \retval  OV_EFAULT  Internal logic fault; indicates a bug or heap/stack corruption.
 * \retval  OV_EINVAL  Invalid setup request, eg, out of range argument.
 * \retval  OV_EIMPL   Unimplemented mode; unable to comply with quality level request.
 */
int vorbis_encode_setup_vbr(vorbis_info* vi,
                            long channels,
                            long rate,
                            float quality)
{
    codec_setup_info* ci;
    highlevel_encode_setup* hi;
    if (rate <= 0) return OV_EINVAL;

    ci = vi->codec_setup;
    hi = &ci->hi;

    quality += .0000001;
    if (quality >= 1.) quality = .9999;

    hi->req = quality;
    hi->setup = get_setup_template(channels, rate, quality, 0, &hi->base_setting);
    if (!hi->setup) return OV_EIMPL;

    vorbis_encode_setup_setting(vi, channels, rate);
    hi->managed = 0;
    hi->coupling_p = 1;

    return 0;
}

/**
 * This is the primary function within libvorbisenc for setting up variable
 * bitrate ("quality" based) modes.
 *
 *
 * Before this function is called, the vorbis_info struct should be
 * initialized by using vorbis_info_init() from the libvorbis API. After
 * encoding, vorbis_info_clear() should be called.
 *
 * \param vi           Pointer to an initialized vorbis_info struct.
 * \param channels     The number of channels to be encoded.
 * \param rate         The sampling rate of the source audio.
 * \param base_quality Desired quality level, currently from -0.1 to 1.0 (lo to hi).
 *
 *
 * \return Zero for success, or a negative number for failure.
 *
 * \retval 0           Success
 * \retval OV_EFAULT   Internal logic fault; indicates a bug or heap/stack corruption.
 * \retval OV_EINVAL   Invalid setup request, eg, out of range argument.
 * \retval OV_EIMPL    Unimplemented mode; unable to comply with quality level request.
 */
int vorbis_encode_init_vbr(vorbis_info* vi,
                           long channels,
                           long rate,

                           float base_quality /* 0. to 1. */
                           )
{
    int ret = 0;

    ret = vorbis_encode_setup_vbr(vi, channels, rate, base_quality);

    if (ret)
    {
        vorbis_info_clear(vi);
        return ret;
    }
    ret = vorbis_encode_setup_init(vi);
    if (ret)
        vorbis_info_clear(vi);
    return (ret);
}

/**
 * This function performs step-one of a three-step bitrate-managed encode
 * setup.  It functions similarly to the one-step setup performed by \ref
 * vorbis_encode_init but allows an application to make further encode setup
 * tweaks using \ref vorbis_encode_ctl before finally calling \ref
 * vorbis_encode_setup_init to complete the setup process.
 *
 * Before this function is called, the \ref vorbis_info struct should be
 * initialized by using vorbis_info_init() from the libvorbis API.  After
 * encoding, vorbis_info_clear() should be called.
 *
 * The max_bitrate, nominal_bitrate, and min_bitrate settings are used to set
 * constraints for the encoded file.  This function uses these settings to
 * select the appropriate encoding mode and set it up.
 *
 * \param vi                Pointer to an initialized vorbis_info struct.
 * \param channels          The number of channels to be encoded.
 * \param rate              The sampling rate of the source audio.
 * \param max_bitrate       Desired maximum bitrate (limit). -1 indicates unset.
 * \param nominal_bitrate   Desired average, or central, bitrate. -1 indicates unset.
 * \param min_bitrate       Desired minimum bitrate. -1 indicates unset.
 *
 * \return Zero for success, and negative for failure.
 *
 * \retval 0           Success
 * \retval OV_EFAULT   Internal logic fault; indicates a bug or heap/stack corruption.
 * \retval OV_EINVAL   Invalid setup request, eg, out of range argument.
 * \retval OV_EIMPL    Unimplemented mode; unable to comply with bitrate request.
 */
int vorbis_encode_setup_managed(vorbis_info* vi,
                                long channels,
                                long rate,

                                long max_bitrate,
                                long nominal_bitrate,
                                long min_bitrate)
{

    codec_setup_info* ci;
    highlevel_encode_setup* hi;
    double tnominal;
    if (rate <= 0) return OV_EINVAL;

    ci = vi->codec_setup;
    hi = &ci->hi;
    tnominal = nominal_bitrate;

    if (nominal_bitrate <= 0.)
    {
        if (max_bitrate > 0.)
        {
            if (min_bitrate > 0.)
                nominal_bitrate = (max_bitrate + min_bitrate) * .5;
            else
                nominal_bitrate = max_bitrate * .875;
        }
        else
        {
            if (min_bitrate > 0.)
            {
                nominal_bitrate = min_bitrate;
            }
            else
            {
                return (OV_EINVAL);
            }
        }
    }

    hi->req = nominal_bitrate;
    hi->setup = get_setup_template(channels, rate, nominal_bitrate, 1, &hi->base_setting);
    if (!hi->setup) return OV_EIMPL;

    vorbis_encode_setup_setting(vi, channels, rate);

    /* initialize management with sane defaults */
    hi->coupling_p = 1;
    hi->managed = 1;
    hi->bitrate_min = min_bitrate;
    hi->bitrate_max = max_bitrate;
    hi->bitrate_av = tnominal;
    hi->bitrate_av_damp = 1.5f; /* full range in no less than 1.5 second */
    hi->bitrate_reservoir = nominal_bitrate * 2;
    hi->bitrate_reservoir_bias = .1; /* bias toward hoarding bits */

    return (0);

}

/**
 * This is the primary function within libvorbisenc for setting up managed
 * bitrate modes.
 *
 * Before this function is called, the \ref vorbis_info
 * struct should be initialized by using vorbis_info_init() from the libvorbis
 * API.  After encoding, vorbis_info_clear() should be called.
 *
 * The max_bitrate, nominal_bitrate, and min_bitrate settings are used to set
 * constraints for the encoded file.  This function uses these settings to
 * select the appropriate encoding mode and set it up.
 *
 * \param vi               Pointer to an initialized \ref vorbis_info struct.
 * \param channels         The number of channels to be encoded.
 * \param rate             The sampling rate of the source audio.
 * \param max_bitrate      Desired maximum bitrate (limit). -1 indicates unset.
 * \param nominal_bitrate  Desired average, or central, bitrate. -1 indicates unset.
 * \param min_bitrate      Desired minimum bitrate. -1 indicates unset.
 *
 * \return Zero for success, and negative values for failure.
 *
 * \retval 0          Success.
 * \retval OV_EFAULT  Internal logic fault; indicates a bug or heap/stack corruption.
 * \retval OV_EINVAL  Invalid setup request, eg, out of range argument.
 * \retval OV_EIMPL   Unimplemented mode; unable to comply with bitrate request.
 */
int vorbis_encode_init(vorbis_info* vi,
                       long channels,
                       long rate,

                       long max_bitrate,
                       long nominal_bitrate,
                       long min_bitrate)
{

    int ret = vorbis_encode_setup_managed(vi, channels, rate,
                                        max_bitrate,
                                        nominal_bitrate,
                                        min_bitrate);
    if (ret)
    {
        vorbis_info_clear(vi);
        return (ret);
    }

    ret = vorbis_encode_setup_init(vi);
    if (ret)
        vorbis_info_clear(vi);
    return (ret);
}

/**
 * This function implements a generic interface to miscellaneous encoder
 * settings similar to the classic UNIX 'ioctl()' system call.  Applications
 * may use vorbis_encode_ctl() to query or set bitrate management or quality
 * mode details by using one of several \e request arguments detailed below.
 * vorbis_encode_ctl() must be called after one of
 * vorbis_encode_setup_managed() or vorbis_encode_setup_vbr().  When used
 * to modify settings, \ref vorbis_encode_ctl() must be called before \ref
 * vorbis_encode_setup_init().
 *
 * \param vi      Pointer to an initialized vorbis_info struct.
 *
 * \param number Specifies the desired action; See \ref encctlcodes "the list
 * of available requests".
 *
 * \param arg void * pointing to a data structure matching the request
 * argument.
 *
 * \retval 0          Success. Any further return information (such as the result of a
 * query) is placed into the storage pointed to by *arg.
 *
 * \retval OV_EINVAL  Invalid argument, or an attempt to modify a setting after
 * calling vorbis_encode_setup_init().
 *
 * \retval OV_EIMPL   Unimplemented or unknown request
 */
int vorbis_encode_ctl(vorbis_info* vi, int number, void* arg)
{
    if (vi)
    {
        codec_setup_info* ci = vi->codec_setup;
        highlevel_encode_setup* hi = &ci->hi;
        int setp = (number & 0xf); /* a read request has a low nibble of 0 */

        if (setp && hi->set_in_stone) return (OV_EINVAL);

        switch (number)
        {

            /* now deprecated *****************/
            case OV_ECTL_RATEMANAGE_GET:
                {

        struct ovectl_ratemanage_arg * ai =
          (struct ovectl_ratemanage_arg *)arg;

        ai->management_active=hi->managed;
        ai->bitrate_hard_window=ai->bitrate_av_window=
          (double) hi->bitrate_reservoir/vi->rate;
        ai->bitrate_av_window_center=1.;
        ai->bitrate_hard_min=hi->bitrate_min;
        ai->bitrate_hard_max=hi->bitrate_max;
        ai->bitrate_av_lo=hi->bitrate_av;
        ai->bitrate_av_hi=hi->bitrate_av;

      }
      return(0);

    /* now deprecated *****************/
    case OV_ECTL_RATEMANAGE_SET:
      {
        struct ovectl_ratemanage_arg * ai =
          (struct ovectl_ratemanage_arg *)arg;
        if(ai==NULL){
          hi->managed=0;
        }else{
          hi->managed=ai->management_active;
          vorbis_encode_ctl(vi, OV_ECTL_RATEMANAGE_AVG, arg);
vorbis_encode_ctl(vi, OV_ECTL_RATEMANAGE_HARD, arg);
        }
      }
      return 0;

    /* now deprecated *****************/
    case OV_ECTL_RATEMANAGE_AVG:
      {
        struct ovectl_ratemanage_arg * ai =
          (struct ovectl_ratemanage_arg *)arg;
        if(ai==NULL){
          hi->bitrate_av=0;
        }else{
          hi->bitrate_av=(ai->bitrate_av_lo+ai->bitrate_av_hi)*.5;
        }
      }
      return(0);
    /* now deprecated *****************/
    case OV_ECTL_RATEMANAGE_HARD:
      {
        struct ovectl_ratemanage_arg * ai =
          (struct ovectl_ratemanage_arg *)arg;
        if(ai==NULL){
          hi->bitrate_min=0;
          hi->bitrate_max=0;
        }else{
          hi->bitrate_min=ai->bitrate_hard_min;
          hi->bitrate_max=ai->bitrate_hard_max;
          hi->bitrate_reservoir=ai->bitrate_hard_window*
            (hi->bitrate_max+hi->bitrate_min)*.5;
        }
        if(hi->bitrate_reservoir<128.)
          hi->bitrate_reservoir=128.;
      }
      return(0);

      /* replacement ratemanage interface */
    case OV_ECTL_RATEMANAGE2_GET:
      {
        struct ovectl_ratemanage2_arg * ai =
          (struct ovectl_ratemanage2_arg *)arg;
        if(ai==NULL)return OV_EINVAL;

        ai->management_active=hi->managed;
        ai->bitrate_limit_min_kbps=hi->bitrate_min/1000;
        ai->bitrate_limit_max_kbps=hi->bitrate_max/1000;
        ai->bitrate_average_kbps=hi->bitrate_av/1000;
        ai->bitrate_average_damping=hi->bitrate_av_damp;
        ai->bitrate_limit_reservoir_bits=hi->bitrate_reservoir;
        ai->bitrate_limit_reservoir_bias=hi->bitrate_reservoir_bias;
      }
      return (0);
    case OV_ECTL_RATEMANAGE2_SET:
      {
        struct ovectl_ratemanage2_arg * ai =
          (struct ovectl_ratemanage2_arg *)arg;
        if(ai==NULL){
          hi->managed=0;
        }else{
          /* sanity check; only catch invariant violations */
          if(ai->bitrate_limit_min_kbps>0 &&
             ai->bitrate_average_kbps>0 &&
             ai->bitrate_limit_min_kbps>ai->bitrate_average_kbps)
            return OV_EINVAL;

          if(ai->bitrate_limit_max_kbps>0 &&
             ai->bitrate_average_kbps>0 &&
             ai->bitrate_limit_max_kbps<ai->bitrate_average_kbps)
            return OV_EINVAL;

          if(ai->bitrate_limit_min_kbps>0 &&
             ai->bitrate_limit_max_kbps>0 &&
             ai->bitrate_limit_min_kbps>ai->bitrate_limit_max_kbps)
            return OV_EINVAL;

          if(ai->bitrate_average_damping <= 0.)
            return OV_EINVAL;

          if(ai->bitrate_limit_reservoir_bits< 0)
            return OV_EINVAL;

          if(ai->bitrate_limit_reservoir_bias< 0.)
            return OV_EINVAL;

          if(ai->bitrate_limit_reservoir_bias > 1.)
            return OV_EINVAL;

          hi->managed=ai->management_active;
          hi->bitrate_min=ai->bitrate_limit_min_kbps* 1000;
          hi->bitrate_max=ai->bitrate_limit_max_kbps* 1000;
          hi->bitrate_av=ai->bitrate_average_kbps* 1000;
          hi->bitrate_av_damp=ai->bitrate_average_damping;
          hi->bitrate_reservoir=ai->bitrate_limit_reservoir_bits;
          hi->bitrate_reservoir_bias=ai->bitrate_limit_reservoir_bias;
        }
      }
      return 0;

    case OV_ECTL_LOWPASS_GET:
      {
        double* farg = (double*)arg;
        * farg = hi->lowpass_kHz;
      }
      return(0);
    case OV_ECTL_LOWPASS_SET:
      {
        double* farg = (double*)arg;
hi->lowpass_kHz=* farg;

        if(hi->lowpass_kHz<2.)hi->lowpass_kHz=2.;
        if(hi->lowpass_kHz>99.)hi->lowpass_kHz=99.;
        hi->lowpass_altered=1;
      }
      return(0);
    case OV_ECTL_IBLOCK_GET:
      {
        double* farg = (double*)arg;
        * farg = hi->impulse_noisetune;
      }
      return(0);
    case OV_ECTL_IBLOCK_SET:
      {
        double* farg = (double*)arg;
hi->impulse_noisetune=* farg;

        if(hi->impulse_noisetune>0.)hi->impulse_noisetune=0.;
        if(hi->impulse_noisetune<-15.)hi->impulse_noisetune=-15.;
      }
      return(0);
    case OV_ECTL_COUPLING_GET:
      {
        int* iarg = (int*)arg;
        * iarg = hi->coupling_p;
      }
      return(0);
    case OV_ECTL_COUPLING_SET:
      {
        const void* new_template;
double new_base = 0.;
int* iarg = (int*)arg;
hi->coupling_p=((* iarg)!=0);

        /* Fetching a new template can alter the base_setting, which
           many other parameters are based on.  Right now, the only
           parameter drawn from the base_setting that can be altered
           by an encctl is the lowpass, so that is explictly flagged
           to not be overwritten when we fetch a new template and
           recompute the dependant settings */
        new_template = get_setup_template(hi->coupling_p? vi->channels:-1,
                                          vi->rate,
                                          hi->req,
                                          hi->managed,
                                          &new_base);
        if(!hi->setup)return OV_EIMPL;
        hi->setup=new_template;
        hi->base_setting=new_base;
        vorbis_encode_setup_setting(vi, vi->channels, vi->rate);
      }
      return(0);
    }
    return(OV_EIMPL);
  }
  return(OV_EINVAL);
}

/**
 * \deprecated This is a deprecated interface. Please use vorbis_encode_ctl()
 * with the \ref ovectl_ratemanage2_arg struct and \ref
 * OV_ECTL_RATEMANAGE2_GET and \ref OV_ECTL_RATEMANAGE2_SET calls in new code.
 *
 * The \ref ovectl_ratemanage_arg structure is used with vorbis_encode_ctl()
 * and the \ref OV_ECTL_RATEMANAGE_GET, \ref OV_ECTL_RATEMANAGE_SET, \ref
 * OV_ECTL_RATEMANAGE_AVG, \ref OV_ECTL_RATEMANAGE_HARD calls in order to
 * query and modify specifics of the encoder's bitrate management
 * configuration.
*/
struct ovectl_ratemanage_arg
{
    int management_active; /**< nonzero if bitrate management is active*/
    /** hard lower limit (in kilobits per second) below which the stream bitrate
        will never be allowed for any given bitrate_hard_window seconds of time.*/
    long bitrate_hard_min;
    /** hard upper limit (in kilobits per second) above which the stream bitrate
        will never be allowed for any given bitrate_hard_window seconds of time.*/
    long bitrate_hard_max;
    /** the window period (in seconds) used to regulate the hard bitrate minimum
        and maximum*/
    double bitrate_hard_window;
    /** soft lower limit (in kilobits per second) below which the average bitrate
        tracker will start nudging the bitrate higher.*/
    long bitrate_av_lo;
    /** soft upper limit (in kilobits per second) above which the average bitrate
        tracker will start nudging the bitrate lower.*/
    long bitrate_av_hi;
    /** the window period (in seconds) used to regulate the average bitrate
        minimum and maximum.*/
    double bitrate_av_window;
    /** Regulates the relative centering of the average and hard windows; in
        libvorbis 1.0 and 1.0.1, the hard window regulation overlapped but
        followed the average window regulation. In libvorbis 1.1 a bit-reservoir
        interface replaces the old windowing interface; the older windowing
        interface is simulated and this field has no effect.*/
    double bitrate_av_window_center;
};

/**
 * \name struct ovectl_ratemanage2_arg
 *
 * The ovectl_ratemanage2_arg structure is used with vorbis_encode_ctl() and
 * the OV_ECTL_RATEMANAGE2_GET and OV_ECTL_RATEMANAGE2_SET calls in order to
 * query and modify specifics of the encoder's bitrate management
 * configuration.
 *
*/
struct ovectl_ratemanage2_arg
{
    int management_active; /**< nonzero if bitrate management is active */
    /** Lower allowed bitrate limit in kilobits per second */
    long bitrate_limit_min_kbps;
    /** Upper allowed bitrate limit in kilobits per second */
    long bitrate_limit_max_kbps;
    long bitrate_limit_reservoir_bits; /**<Size of the bitrate reservoir in bits */
    /** Regulates the bitrate reservoir's preferred fill level in a range from 0.0
     * to 1.0; 0.0 tries to bank bits to buffer against future bitrate spikes, 1.0
     * buffers against future sudden drops in instantaneous bitrate. Default is
     * 0.1
     */
    double bitrate_limit_reservoir_bias;
    /** Average bitrate setting in kilobits per second */
    long bitrate_average_kbps;
    /** Slew rate limit setting for average bitrate adjustment; sets the minimum
     *  time in seconds the bitrate tracker may swing from one extreme to the
     *  other when boosting or damping average bitrate.
     */
    double bitrate_average_damping;
};

/**
 * \name vorbis_encode_ctl() codes
 *
 * \anchor encctlcodes
 *
 * These values are passed as the \c number parameter of vorbis_encode_ctl().
 * The type of the referent of that function's \c arg pointer depends on these
 * codes.
 */
/*@{*/

/**
 * Query the current encoder bitrate management setting.
 *
 *Argument: <tt>struct ovectl_ratemanage2_arg *</tt>
 *
 * Used to query the current encoder bitrate management setting. Also used to
 * initialize fields of an ovectl_ratemanage2_arg structure for use with
 * \ref OV_ECTL_RATEMANAGE2_SET.
 */
#define OV_ECTL_RATEMANAGE2_GET      0x14

/**
 * Set the current encoder bitrate management settings.
 *
 * Argument: <tt>struct ovectl_ratemanage2_arg *</tt>
 *
 * Used to set the current encoder bitrate management settings to the values
 * listed in the ovectl_ratemanage2_arg. Passing a NULL pointer will disable
 * bitrate management.
*/
#define OV_ECTL_RATEMANAGE2_SET      0x15

/**
 * Returns the current encoder hard-lowpass setting (kHz) in the double
 * pointed to by arg.
 *
 * Argument: <tt>double *</tt>
*/
#define OV_ECTL_LOWPASS_GET          0x20

/**
 *  Sets the encoder hard-lowpass to the value (kHz) pointed to by arg. Valid
 *  lowpass settings range from 2 to 99.
 *
 * Argument: <tt>double *</tt>
*/
#define OV_ECTL_LOWPASS_SET          0x21

/**
 *  Returns the current encoder impulse block setting in the double pointed
 *  to by arg.
 *
 * Argument: <tt>double *</tt>
*/
#define OV_ECTL_IBLOCK_GET           0x30

/**
 *  Sets the impulse block bias to the the value pointed to by arg.
 *
 * Argument: <tt>double *</tt>
 *
 *  Valid range is -15.0 to 0.0 [default]. A negative impulse block bias will
 *  direct to encoder to use more bits when incoding short blocks that contain
 *  strong impulses, thus improving the accuracy of impulse encoding.
 */
#define OV_ECTL_IBLOCK_SET           0x31

/**
 *  Returns the current encoder coupling setting in the int pointed
 *  to by arg.
 *
 * Argument: <tt>int *</tt>
*/
#define OV_ECTL_COUPLING_GET         0x40

/**
 *  Enables/disables channel coupling in multichannel encoding according to arg.
 *
 * Argument: <tt>int *</tt>
 *
 *  Zero disables channel coupling for multichannel inputs, nonzer enables
 *  channel coupling.  Setting has no effect on monophonic encoding or
 *  multichannel counts that do not offer coupling.  At present, coupling is
 *  available for stereo and 5.1 encoding.
 */
#define OV_ECTL_COUPLING_SET         0x41

  /* deprecated rate management supported only for compatibility */

/**
 * Old interface to querying bitrate management settings.
 *
 * Deprecated after move to bit-reservoir style management in 1.1 rendered
 * this interface partially obsolete.

 * \deprecated Please use \ref OV_ECTL_RATEMANAGE2_GET instead.
 *
 * Argument: <tt>struct ovectl_ratemanage_arg *</tt>
 */
#define OV_ECTL_RATEMANAGE_GET       0x10
/**
 * Old interface to modifying bitrate management settings.
 *
 *  deprecated after move to bit-reservoir style management in 1.1 rendered
 *  this interface partially obsolete.
 *
 * \deprecated Please use \ref OV_ECTL_RATEMANAGE2_SET instead.
 *
 * Argument: <tt>struct ovectl_ratemanage_arg *</tt>
 */
#define OV_ECTL_RATEMANAGE_SET       0x11
/**
 * Old interface to setting average-bitrate encoding mode.
 *
 * Deprecated after move to bit-reservoir style management in 1.1 rendered
 * this interface partially obsolete.
 *
 *  \deprecated Please use \ref OV_ECTL_RATEMANAGE2_SET instead.
 *
 * Argument: <tt>struct ovectl_ratemanage_arg *</tt>
 */
#define OV_ECTL_RATEMANAGE_AVG       0x12
/**
 * Old interface to setting bounded-bitrate encoding modes.
 *
 * deprecated after move to bit-reservoir style management in 1.1 rendered
 * this interface partially obsolete.
 *
 *  \deprecated Please use \ref OV_ECTL_RATEMANAGE2_SET instead.
 *
 * Argument: <tt>struct ovectl_ratemanage_arg *</tt>
 */
#define OV_ECTL_RATEMANAGE_HARD      0x13

/*@}*/












    }
}
