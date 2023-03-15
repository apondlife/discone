#ifndef CORE_COLOR_HLSL
#define CORE_COLOR_HLSL

#include "./Math.hlsl"

// -- queries --
/// layer whichever color has higher alpha
inline fixed4 Layer(fixed4 c1, fixed4 c2) {
    return lerp(c1, c2, step(c1.a, c2.a));
}

/// layer whichever color has higher alpha
inline fixed4 Layer(fixed4 c1, fixed4 c2, fixed4 c3) {
    return Layer(Layer(c1, c2), c3);
}

// -- hsv --
// TODO: shouldn't color all be fixed -ty
float3 LerpHsv(float3 a, float3 b, float1 t) {
    float1 ax = a.x;
    float1 bx = lerp(
        b.x + 1,
        b.x,
        step(a.x, b.x)
    );

    float1 h = lerp(
        lerp(ax, bx, t),
        frac(lerp(ax, bx - 1, t) + 1),
        step(0.5, abs(bx - ax))
    );

    float2 sv = lerp(
        a.yz,
        b.yz,
        t
    );

    return float3(h, sv);
}


/// convert rgb color into hsv
/// see: https://stackoverflow.com/questions/15095909/from-rgb-to-hsv-in-opengl-glsl
float3 IntoHsv(float3 c) {
    float4 K = float4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

/// convert hsv color into rgb
/// https://stackoverflow.com/questions/15095909/from-rgb-to-hsv-in-opengl-glsl
float3 IntoRgb(float3 c) {
    c.y = clamp(c.y, 0.0f, 1.0f);
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

/// get the lumninance from an rgb color using an arbitrary mix found on the internet
/// at some point
float1 GetLuminance(float3 c) {
    return dot(c, float3(0.3f, 0.6f, 0.11f));
}

#endif