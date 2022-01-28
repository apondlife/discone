using UnityEngine;

namespace ThirdPerson {

// a capsule defined by two points and a radius (for capsule casts)
readonly struct Capsule {
    // -- props --
    /// the center of the sphere at the start of the capsule
    public readonly Vector3 Point1;

    /// the center of the sphere at the end of the capsule
    public readonly Vector3 Point2;

    /// the radius of the sphere defining this capsule
    public readonly float Radius;

    // -- lifetime --
    /// create a capsule
    public Capsule(Vector3 point1, Vector3 point2, float radius) {
        Point1 = point1;
        Point2 = point2;
        Radius = radius;
    }

    // -- operators --
    // offset the capsule by the vector
    public Capsule Offset(Vector3 offset) {
        var point1 = Point1 + offset;
        var point2 = Point2 + offset;

        return new Capsule(point1, point2, Radius);
    }

    // -- factories --
    /// create a capsule ray from this capsule
    public Cast IntoCast(Vector3 pos, Vector3 direction, float length) {
        return new Cast(Offset(pos), direction, length);
    }

    /// create a capsule from its center, radius, height, & up vector
    public static Capsule From(Vector3 center, float radius, float height, Vector3 up) {
        var offset = (height * 0.5f - radius) * up;
        var point1 = center - offset;
        var point2 = center + offset;

        return new Capsule(point1, point2, radius);
    }

    // -- cast --
    /// the properties of a capsule cast
    /// TODO: this name is not accurate, but...
    public readonly struct Cast {
        // -- props --
        /// the capsule to cast
        public readonly Capsule Capsule;

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
        }

        // -- queries --
        /// the center of the sphere at the start of the capsule
        public Vector3 Point1 {
            get => Capsule.Point1;
        }

        /// the center of the sphere at the end of the capsule
        public Vector3 Point2 {
            get => Capsule.Point2;
        }

        /// the radius of the sphere defining this capsule
        public float Radius {
            get => Capsule.Radius;
        }
    }
}

}