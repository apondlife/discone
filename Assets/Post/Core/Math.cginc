/// inverse lerp a value (v) given a min (a) and max (b): (a, b) -> (0, 1)
float3 Unlerp(float a, float b, float v) {
    return (v - a) / (b - a);
}