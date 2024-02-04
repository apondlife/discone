#ifndef FOG_HLSL
#define FOG_HLSL

// -- includes --
#include "./Core/Math.hlsl"

// -- props --
// .
fixed4 _HeightFog_Color;

// .
float1 _HeightFog_Density;

// .
float1 _HeightFog_MinDist;

// -- commands --
#define HEIGHT_FOG_COORDS(idx) \
    float1 heightFogCoord : TEXCOORD##idx;

#define TRANSFER_HEIGHT_FOG(o, worldPos); \
    o.heightFogCoord = abs(worldPos.y - _WorldSpaceCameraPos.y);

/// add height fog to the color based on distance
#define APPLY_HEIGHT_FOG(coord, col) \
    col##.rgb = lerp(\
        col##.rgb, \
        _HeightFog_Color, \
        (1 - GetHeightFog(coord)) * _HeightFog_Color.a \
    );

// -- queries --
/// get the fog scale for the given distance
half1 GetHeightFog(float1 dist) {
    half1 fog = _HeightFog_Density * max(0, dist - _HeightFog_MinDist);;
    fog = saturate(exp2(-fog * fog));
    return fog;
}

#endif