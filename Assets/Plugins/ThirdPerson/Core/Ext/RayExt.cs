using UnityEngine;

namespace ThirdPerson {

static class RayExt {
    /// find the intersection of two lines
    /// see: https://stackoverflow.com/questions/59449628/check-when-two-vector3-lines-intersect-unity3d
    public static bool TryIntersect(this Ray a, Ray b, out Vector3 intersection){
        Vector3 c = b.origin - a.origin;
        Vector3 axb = Vector3.Cross(a.direction, b.direction);
        Vector3 cxb = Vector3.Cross(c, b.direction);

        // if the rays are coplanar and nonparallel
        var isIntersecting = (
            Mathf.Abs(Vector3.Dot(c.normalized, axb)) < 0.0001f &&
            axb.sqrMagnitude > 0.0001f
        );

        // if no intersection, return nothing
        if (!isIntersecting) {
            intersection = Vector3.zero;
            return false;
        }

        // otherwise, find the intersection
        float s = Vector3.Dot(cxb, axb) / axb.sqrMagnitude;
        intersection = a.origin + (a.direction * s);

        return true;
    }
}

}
