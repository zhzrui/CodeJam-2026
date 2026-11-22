// PixelArtQuantize.hlsl
// Custom Function node file for a URP Fullscreen Shader Graph.
// Two functions:
//   PixelateUV_float        -> snaps screen UVs to a block grid (point downsample + block upscale in one step)
//   QuantizeToPalette_float -> snaps a colour to the nearest entry of a palette texture (CIE Lab or RGB distance)

#ifndef PIXEL_ART_QUANTIZE_INCLUDED
#define PIXEL_ART_QUANTIZE_INCLUDED

// The next three functions are helpers
float3 PAQ_LinearToSRGB(float3 c)
{
    c = saturate(c);
    return lerp(c * 12.92, 1.055 * pow(c, 1.0 / 2.4) - 0.055, step(0.0031308, c));
}

float3 PAQ_SRGBToLinear(float3 c)
{
    return lerp(c / 12.92, pow((c + 0.055) / 1.055, 2.4), step(0.04045, c));
}

// 4x4 Bayer matrix, returns a threshold in (-0.5, 0.5)
float PAQ_Bayer4x4(float2 blockCoord)
{
    static const float bayer[16] =
    {
        0.0,  8.0,  2.0, 10.0,
       12.0,  4.0, 14.0,  6.0,
        3.0, 11.0,  1.0,  9.0,
       15.0,  7.0, 13.0,  5.0
   };
    uint2 p = (uint2)blockCoord & 3u;
    return (bayer[p.y * 4 + p.x] + 0.5) / 16.0 - 0.5;
}

// ----- THE ACTUAL FUNCTIONS USED IN THE NODES START HERE -------

// 1) Pixelation:
// Returns the UV of the centre of the block this fragment belongs to, so
// every fragment in a block samples the same source pixel (= point filtering).

void PixelateUV_float(float2 UV, float2 ScreenSize, float PixelSize,
                      out float2 SnappedUV, out float2 BlockCoord)
{
    float size = max(1.0, PixelSize);
    float2 pixelCoord = UV * ScreenSize;
    BlockCoord = floor(pixelCoord / size);

    float2 blockCentre = (BlockCoord + 0.5) * size;
    blockCentre = min(blockCentre, ScreenSize - 0.5); // keep the last partial block on screen
    SnappedUV = blockCentre / ScreenSize;
}

// 2) quantization
void QuantizeLevels_float(float3 Color, float Levels, float2 BlockCoord, float DitherStrength,
                          out float3 Out)
{
    float steps = max(2.0, round(Levels)) - 1.0;
    float3 srgb = PAQ_LinearToSRGB(Color);

    // Nudge by up to +/- half a step before rounding -> ordered dither
    srgb += PAQ_Bayer4x4(BlockCoord) * saturate(DitherStrength) / steps;

    srgb = saturate(round(srgb * steps) / steps);
    Out = PAQ_SRGBToLinear(srgb);
}

#endif // PIXEL_ART_QUANTIZE_INCLUDED
