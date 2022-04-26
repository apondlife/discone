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

    /// find the intersection of a line and the incidence plane defined by a tangent ray, b. the
    /// incididence plane both contains b and is orthogonal to the plane containing a & b.
    /// see: https://en.wikipedia.org/wiki/Plane_of_incidence
    /// TODO (partially resolved): what is a math word for this/
    public static bool TryIntersectIncidencePlane(this Ray a, Ray b, out Vector3 intersection) {
        // find a tangent vector on the incidence plane, and then its normal
        var t = Vector3.Cross(b.direction, a.direction);
        var n = Vector3.Cross(b.direction, t);

        // then intersect the ray and the incidence plane
        var plane = new Plane(n, b.origin);
        if (!plane.Raycast(a, out var distance)) {
            intersection = Vector3.zero;
            // TODO: maybe try intersecting with another plane, perpendicular to the first
            // TODO: maybe try pointing a in the other direction as well
            return false;
        }

        intersection = a.origin + distance * a.direction;
        return true;
    }
}

}
