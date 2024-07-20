#ifndef DISTORTION_H
#define DISTORTION_H

// -- includes --
/// .
float3 _Character_Pos;

/// .
float1 _Distortion_AxialScale;

/// .
float1 _Distortion_RadialScale;

/// intensity is in the range [0, 1, infinity]; 0 is fully squashed, 1 is no
/// distortion, infinity is infinitely stretched.
float1 _Distortion_Intensity;

/// the bottom plane of the distortion; verts above this plane distort
float4 _Distortion_BotPlane;

/// the top plane of the distortion; verts below this plane distort, verts above this plane _only_ stretch
float4 _Distortion_TopPlane;

// -- commands --
void Distort(inout float3 worldPos) {
    float3 botAxisDir = _Distortion_BotPlane.xyz;
    float3 topAxisDir = _Distortion_TopPlane.xyz;

    // get axial distortion
    float1 botAxialDistance = dot(botAxisDir, worldPos) + _Distortion_BotPlane.w;
    float1 topAxialDistance = max(dot(topAxisDir, worldPos) + _Distortion_TopPlane.w, 0);
    topAxialDistance *= step(_Distortion_Intensity, 1);

    float1 axialDistance = botAxialDistance - topAxialDistance;
    float3 axialDisplacement = axialDistance * botAxisDir;
    float1 axialDistortionMag = _Distortion_AxialScale * (_Distortion_Intensity - 1);
    float3 axialDistortion =  axialDisplacement * axialDistortionMag;

    // get radial distortion
    float3 displacement = worldPos - _Character_Pos;
    float3 radialDisplacement = displacement - dot(displacement, botAxisDir) * botAxisDir;
    float1 radialDistortionMag = _Distortion_RadialScale * (1 - _Distortion_Intensity);
    float3 radialDistortion = radialDisplacement * radialDistortionMag;

    worldPos += axialDistortion;
    worldPos += radialDistortion;
}

#endif