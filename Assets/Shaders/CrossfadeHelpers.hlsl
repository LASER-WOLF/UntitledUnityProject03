// Make sure this file is not included twice
#ifndef CROSSFADE_HELPERS_INCLUDED
#define CROSSFADE_HELPERS_INCLUDED

//  LOD cross fade helpers
// keep all the old macros
#define UNITY_DITHER_CROSSFADE_COORDS
#define UNITY_DITHER_CROSSFADE_COORDS_IDX(idx)
#define UNITY_TRANSFER_DITHER_CROSSFADE(o,v)
#define UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o,hpos)

#ifdef LOD_FADE_CROSSFADE
    #define UNITY_APPLY_DITHER_CROSSFADE(vpos)  UnityApplyDitherCrossFade(vpos)
    sampler2D unity_DitherMask;
    void UnityApplyDitherCrossFade(float2 vpos)
    {
        vpos /= 4; // the dither mask texture is 4x4
        float mask = tex2D(unity_DitherMask, vpos).a;
        float sgn = unity_LODFade.x > 0 ? 1.0f : -1.0f;
        clip(unity_LODFade.x - mask * sgn);
    }
#else
    #define UNITY_APPLY_DITHER_CROSSFADE(vpos)
#endif

#endif