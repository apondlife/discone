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

/// .
float4 _Distortion_Plane;

// -- commands --
void Distort(inout float3 worldPos) {
    float3 axisDir = _Distortion_Plane.xyz;

    // get axial distortion
    float1 axialDistance = dot(worldPos, axisDir) + _Distortion_Plane.w;
    float3 axialDisplacement = axialDistance * axisDir;
    float1 axialDistortionMag = _Distortion_AxialScale * (_Distortion_Intensity - 1);
    float3 axialDistortion =  axialDisplacement * axialDistortionMag;

    // get radial distortion
    float3 displacement = worldPos - _Character_Pos;
    float3 radialDisplacement = displacement - dot(displacement, axisDir) * axisDir;
    float1 radialDistortionMag = _Distortion_RadialScale * (1 - _Distortion_Intensity);
    float3 radialDistortion = radialDisplacement * radialDistortionMag;

    worldPos += axialDistortion;
    worldPos += radialDistortion;
}

#endif