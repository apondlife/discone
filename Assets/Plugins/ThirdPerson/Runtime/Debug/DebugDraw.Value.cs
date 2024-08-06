using System;
using System.Collections.Generic;
using Soil;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ThirdPerson {

public partial class DebugDraw {
    /// a value that is buffered over n frames
    [Serializable]
    public sealed class Value {
        // -- fields --
        [Tooltip("the rendered color")]
        [SerializeField] Gradient m_Color;

        [Tooltip("the index range of values to draw")]
        [Soil.Range(0, BufferLen)]
        [SerializeField] IntRange m_Range = new(0, BufferLen);

        [Tooltip("the line width")]
        [SerializeField] float m_Width;

        [Tooltip("the length scale")]
        [SerializeField] float m_Scale;

        [Tooltip("if the value is enabled")]
        [SerializeField] bool m_IsEnabled;

        // -- props --
        /// the buffer of values
        Ring<Data> m_Buffer;

        // -- lifetime --
        public Value() {
            m_IsEnabled = true;
            m_Buffer = new Ring<Data>(BufferLen);
            m_Buffer.Add(Data.Empty);
        }

        // -- commands --
        /// initialize the value's config
        public void Init(
            Color color,
            int count,
            float width,
            float scale,
            float minAlpha
        ) {
            var gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[] { new(color, 0f) };
            gradient.alphaKeys = new GradientAlphaKey[] { new(minAlpha, 0f), new (1f, 1f) };

            m_Color = gradient;
            m_Range = new IntRange(0, count);
            m_Width = width;
            m_Scale = scale;
        }

        /// add an empty entry, in case nothing is drawn
        public void Prepare() {
            m_Buffer.Add(Data.Empty);
        }

        /// clear the value
        public void Clear() {
            m_Buffer.Clear();
        }

        /// add a bool entry to this value
        public void Bool(bool value) {
            Set(new Data(
                DataType.Bool,
                scalar: value ? 1f : 0f,
                vector: DebugDraw.Ray.Zero
            ));
        }

        /// add an int entry to this value
        public void Int(int value) {
            Set(new Data(
                DataType.Int,
                scalar: value,
                vector: DebugDraw.Ray.Zero
            ));
        }

        /// add a float entry to this value
        public void Float(float value) {
            Set(new Data(
                DataType.Float,
                scalar: value,
                vector: DebugDraw.Ray.Zero
            ));
        }

        /// add a point entry to this value
        public void Point(Vector3 value) {
            Set(new Data(
                DataType.Point,
                scalar: 0f,
                vector: new Ray(value, Vector3.zero)
            ));
        }

        /// add a ray entry to this value
        public void Ray(Vector3 pos, Vector3 dir) {
            Set(new Data(
                DataType.Ray,
                scalar: 0f,
                vector: new Ray(pos, dir)
            ));
        }

        /// add a line entry to this value
        public void Line(Vector3 src, Vector3 dst) {
            Set(new Data(
                DataType.Ray,
                scalar: 0f,
                vector: new Ray(src, dst - src)
            ));
        }

        /// add a data entry to this value
        void Set(Data next) {
            m_Buffer[0] = next;
        }

        // -- queries --
        /// if the enabled and in the set of tags
        public bool IsVisible {
            get => m_IsEnabled;
        }

        /// gets the nth-newest value
        public Data this[int offset] {
            get => m_Buffer[offset];
        }

        /// .
        public IEnumerable<Data> All {
            get => m_Buffer;
        }

        ///  .
        public Gradient Color {
            get => m_Color;
        }

        ///  .
        public float Width {
            get => m_Width;
        }

        ///  .
        public float Scale {
            get => m_Scale;
        }

        ///  .
        public IntRange Range {
            get => m_Range;
        }

        ///  .
        public bool IsEnabled {
            get => m_IsEnabled;
            set => m_IsEnabled = value;
        }
    }

    /// the data type of a debug value
    public enum DataType {
        Bool,
        Int,
        Float,
        Point,
        Ray,
    }

    /// the data of a debug value
    public readonly struct Data {
        // -- props --
        /// the configured data type
        public readonly DataType Type;

        /// a scalar value
        public readonly float Scalar;

        /// a vector value
        public readonly Ray Vector;

        // -- lifetime --
        /// initialize the value
        public Data(
            DataType type,
            float scalar,
            Ray vector
        ) {
            Type = type;
            Scalar = scalar;
            Vector = vector;
        }

        // -- queries --
        /// the ray value
        public Ray RayValue {
            get => Vector;
        }

        // -- factories --
        /// an empty value
        public static Data Empty {
            get => new(DataType.Bool, 0, Ray.Zero);
        }
    }

    public readonly struct Ray {
        // -- props --
        /// the starting position
        public readonly Vector3 Pos;

        /// the direction & length
        public readonly Vector3 Dir;

        // -- lifetime --
        public Ray(
            Vector3 pos,
            Vector3 dir
        ) {
            Pos = pos;
            Dir = dir;
        }

        // -- factories --
        /// a zero value
        public static Ray Zero {
            get => new(Vector3.zero, Vector3.zero);
        }
    }
}

}