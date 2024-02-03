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
float1 _HeightFog_MinHeight;

// -- commands --
/// add height fog to the color based on distance
#define ADD_HEIGHT_FOG(dist, col) \
    col##.rgb = lerp(\
        col##.rgb, \
        _HeightFog_Color, \
        GetHeightFog(dist) * _HeightFog_Color.a \
    );

// -- queries --
/// get the fog scale for the given distance
half1 GetHeightFog(float1 dist) {
    dist = max(0, dist - _HeightFog_MinHeight);

    half1 fog = saturate(exp2(-_HeightFog_Density * dist));
    fog  = 1 - fog;
    fog *= step(_Epsilon, dist);

    return fog;
}

#endif