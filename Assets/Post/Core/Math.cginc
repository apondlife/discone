/// inverse lerp a value (v) given a min (a) and max (b): (a, b) -> (0, 1)
float3 Unlerp(float a, float b, float v) {
    return (v - a) / (b - a);
}

/// sample a random value for a 2d coordinate
float Rand(float2 st) {
    return frac(sin(dot(st, float2(12.9898f, 78.233f))) * 43758.5453123f);
}
