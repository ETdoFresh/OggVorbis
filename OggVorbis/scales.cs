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

 function: linear scale -> dB, Bark and Mel scales
  
 Ported by: ETdoFresh

 ********************************************************************/
using System;
using static System.Math;

namespace OggVorbis
{
    public static class scales
    {
        public static float unitnorm(float x)
        {
            if (x < 0) return (-1f);
            return (1f);
        }

        public static float todB(float x) => (float)((x) == 0 ? -400f : Log((x) * (x)) * 4.34294480f);
        public static float todB_nn(float x) => (float)((x) == 0 ? -400f : Log((x)) * 8.6858896f);

        public static float fromdB(float x) => (float)(Exp((x) * .11512925f));

        /* The bark scale equations are approximations, since the original
           table was somewhat hand rolled.  The below are chosen to have the
           best possible fit to the rolled tables, thus their somewhat odd
           appearance (these are more accurate and over a longer range than
           the oft-quoted bark equations found in the texts I have).  The
           approximations are valid from 0 - 30kHz (nyquist) or so.

           all f in Hz, z in Bark */

        public static float toBARK(float n) => (float)(13.1f * Atan(.00074f * (n)) + 2.24f * Atan((n) * (n) * 1.85e-8f) + 1e-4f * (n));
        public static float fromBARK(float z) => (float)(102f * (z) - 2f * Pow(z, 2f) + .4f * Pow(z, 3f) + Pow(1.46f, z) - 1f);
        public static float toMEL(float n) => (float)(Log(1f + (n) * .001f) * 1442.695f);
        public static float fromMEL(float m) => (float)(1000f * Exp((m) / 1442.695f) - 1000f);

        /* Frequency to octave.  We arbitrarily declare 63.5 Hz to be octave
           0.0 */

        public static float toOC(float n) => (float)(Log(n) * 1.442695f - 5.965784f);
        public static float fromOC(float o) => (float)(Exp(((o) + 5.965784f) * .693147f));
    }
}