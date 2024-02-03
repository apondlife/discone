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
/// add height fog to the color based on distance
#define ADD_HEIGHT_FOG(dist, col) \
    col##.rgb = lerp(\
        col##.rgb, \
        _HeightFog_Color, \
        (1 - GetHeightFog(dist)) * _HeightFog_Color.a \
    );

// -- queries --
/// get the fog scale for the given distance
half1 GetHeightFog(float1 dist) {
    half1 fog = _HeightFog_Density * max(0, dist - _HeightFog_MinDist);;
    fog = saturate(exp2(-fog * fog));
    return fog;
}

#endif