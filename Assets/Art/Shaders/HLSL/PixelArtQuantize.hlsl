// PixelArtQuantize.hlsl
// Custom Function node file for a URP Fullscreen Shader Graph.
// Two functions:
//   PixelateUV_float        -> snaps screen UVs to a block grid (point downsample + block upscale in one step)
//   QuantizeToPalette_float -> snaps a colour to the nearest entry of a palette texture (CIE Lab or RGB distance)

#ifndef PIXEL_ART_QUANTIZE_INCLUDED
#define PIXEL_ART_QUANTIZE_INCLUDED

// The next three functions are helpers
float3 PAQ_LinearSRGBToXYZ(float3 rgb)
{
    // Linear sRGB (D65) -> CIE XYZ
    const float3x3 m = float3x3(
        0.4124564, 0.3575761, 0.1804375,
        0.2126729, 0.7151522, 0.0721750,
        0.0193339, 0.1191920, 0.9503041
    );
    return mul(m, rgb);
}

float3 PAQ_XYZToLab(float3 xyz)
{
    // Normalise by D65 white point
    xyz /= float3(0.95047, 1.0, 1.08883);

    // Component-wise piecewise function (written without the vector ternary
    // so it also compiles cleanly under HLSL 2021 rules)
    float3 cubeRoot = pow(max(xyz, 0.0), 1.0 / 3.0);
    float3 linearPart = 7.787 * xyz + 16.0 / 116.0;
    float3 f = lerp(linearPart, cubeRoot, step(0.008856, xyz));

    float L = 116.0 * f.y - 16.0;
    float a = 500.0 * (f.x - f.y);
    float b = 200.0 * (f.y - f.z);
    return float3(L, a, b);
}

float3 PAQ_LinearToLab(float3 rgb)
{
    return PAQ_XYZToLab(PAQ_LinearSRGBToXYZ(saturate(rgb)));
}

// ----- THE ACTUAL FUNCTIONS USED IN THE NODES START HERE -------

// 1) Pixelation:
// Returns the UV of the centre of the block this fragment belongs to, so
// every fragment in a block samples the same source pixel (= point filtering).

void PixelateUV_float(float2 UV, float2 screenSize, float pixelSize, out float2 SnappedUV)
{
    float size = max(1.0, pixelSize);
    float2 pixelCoord = UV * screenSize;
    float2 blockCentre = (floor(pixelCoord / size) + 0.5) * size;
    // Keep the last partial block on screen
    blockCentre = min(blockCentre, screenSize - 0.5);
    SnappedUV = blockCentre / screenSize;
}

// 2) Palette quantization
// The palette size is read from the texture width automatically (max 256).

void QuantizeToPalette_float(float3 Colour, UnityTexture2D Palette, bool ActivateCIELab, out float3 Out)
{
    int count = min((int)Palette.texelSize.z, 256);
    bool lab = ActivateCIELab;
    float3 target = lab ? PAQ_LinearToLab(Colour) : saturate(Colour);

    float bestDist = 1e20;
    float3 bestColor = Colour;

    [loop]
    for (int i = 0; i < count; i++)
    {
        // Load = exact texel fetch, no filtering. sRGB textures come back linear.
        float3 p = LOAD_TEXTURE2D(Palette.tex, int2(i, 0)).rgb;
        float3 pCompare = lab ? PAQ_LinearToLab(p) : p;

        float3 d = target - pCompare;
        float dist = dot(d, d); // squared distance is enough for "nearest"

        if (dist < bestDist)
        {
            bestDist = dist;
            bestColor = p;
        }
    }

    Out = bestColor;
}

#endif // PIXEL_ART_QUANTIZE_INCLUDED
