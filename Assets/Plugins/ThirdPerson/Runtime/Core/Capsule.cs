using UnityEngine;

namespace ThirdPerson {

// a capsule defined by two points and a radius (for capsule casts)
readonly struct Capsule {
    // -- props --
    /// the center of the capsule
    public readonly Vector3 Center;

    /// the radius of the sphere defining this capsule
    public readonly float Radius;

    /// the height of the capsule
    public readonly float Height;

    /// the up direction
    public readonly Vector3 Up;

    // -- lifetime --
    /// create a capsule from its center, radius, height, and up vector
    public Capsule(Vector3 center, float radius, float height, Vector3 up) {
        Center = center;
        Radius = radius;
        Height = height;
        Up = up;
    }

    // -- operators --
    // offset the capsule by the vector
    public Capsule Offset(Vector3 offset) {
        return new Capsule(Center + offset, Radius, Height, Up);
    }

    // -- queries --
    /// find the centers of the start and end spheres
    public (Vector3 point1, Vector3 point2) Points() {
        var offset = (Height * 0.5f - Radius) * Up;
        var point1 = Center - offset;
        var point2 = Center + offset;

        return (point1, point2);
    }

    // -- factories --
    /// create a capsule ray from this capsule
    public Cast IntoCast(
        Vector3 pos,
        Vector3 direction,
        float length
    ) {
        return new Cast(Offset(pos), direction, length);
    }

    // -- cast --
    /// the properties of a capsule cast
    /// TODO: this name is not accurate, but...
    public readonly struct Cast {
        // -- props --
        /// the capsule to cast
        public readonly Capsule Capsule;

        /// the center of the sphere at the start of the capsule
        public readonly Vector3 Point1;

        /// the center of the sphere at the end of the capsule
        public readonly Vector3 Point2;

        /// the direction of the cast
        public readonly Vector3 Direction;

        /// the max length of the cast
        public readonly float Length;

        // -- lifetime --
        /// create a capsule cast
        public Cast(Capsule capsule, Vector3 direction, float length) {
            Capsule = capsule;
            Direction = direction;
            Length = length;

            var (point1, point2) = capsule.Points();
            Point1 = point1;
            Point2 = point2;
        }

        // -- queries --
        /// the radius of the sphere defining this capsule
        public float Radius {
            get => Capsule.Radius;
        }

        /// make a ray from the center of the capsule
        public Ray IntoRay() {
            return new Ray(Capsule.Center, Direction);
        }
    }
}

}