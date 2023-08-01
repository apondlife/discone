using UnityEngine;

namespace ThirdPerson
{
    static class PlaneExt {
        public static Vector4 AsVector4(this Plane plane) {
            return new Vector4(
                plane.normal.x,
                plane.normal.y,
                plane.normal.z,
                plane.distance
            );
        }
    }
}