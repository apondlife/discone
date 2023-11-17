using System;
using UnityEngine;
using System.Collections.Generic;
using Shapes;
using Random = UnityEngine.Random;

namespace  ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw: ImmediateModeShapeDrawer {
    // -- constants --
    const uint k_BufferLen = 300;

    // -- static --
    /// the singleton instance
    static DebugDraw s_Instance;

    // -- props --
    // TODO: make this a keyable type
    [Tooltip("the map of values")]
    [SerializeField] List<Value> m_Values = new();

    // -- lifecycle --
    void Awake() {
        s_Instance = this;
    }

    // -- commands --
    /// push the drawing's next value
    public static void Push(string name, Vector3 pos, Vector3 dir) {
        s_Instance.PushNext(name, pos, dir);
    }

    /// set the drawing's current value
    void PushNext(string name, Vector3 pos, Vector3 dir) {
        // find or create the value
        var value = null as Value;
        foreach (var other in m_Values) {
            if (other.Name == name) {
                value = other;
                break;
            }
        }

        if (value == null) {
            value = new Value(
                name,
                color: Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f),
                width: 1f,
                lengthScale: 1f
            );

            m_Values.Add(value);
        }

        // push the current value
        value.Push(new Ray(pos, dir));
    }

    // -- data --
    /// a value that is buffered over n frames
    [Serializable]
    record Value {
        /// the name of this value
        [SerializeField] [HideInInspector] string m_Name;

        // -- fields --
        [Tooltip("the rendered color")]
        [SerializeField] Color m_Color;

        [Tooltip("the line width")]
        [SerializeField] float m_Width;

        [Tooltip("the length scale")]
        [SerializeField] float m_LengthScale;

        [Tooltip("the n most recent values to draw")]
        [Range(0, k_BufferLen)]
        [SerializeField] uint m_Count;

        [Tooltip("the line width")]
        [SerializeField] bool m_IsEnabled;

        // -- props --
        /// the buffer of values
        Queue<Ray> m_Buffer;

        // -- lifetime --
        public Value(
            string name,
            Color color,
            float width,
            float lengthScale
        ) {
            m_Name = name;
            m_Color = color;
            m_Width = width;
            m_LengthScale = lengthScale;
            m_Count = k_BufferLen;
            m_IsEnabled = true;
            m_Buffer = new Queue<Ray>(k_BufferLen);
        }

        // -- commands --
        /// .
        public void Push(Ray next) {
            m_Buffer.Add(next);
        }

        // -- queries --
        /// gets the nth-newest value
        public Ray this[uint offset] {
            get => m_Buffer[offset];
        }

        /// .
        public string Name {
            get => m_Name;
        }

        /// .
        public IEnumerable<Ray> All {
            get => m_Buffer;
        }

        ///  .
        public Color Color {
            get => m_Color;
        }

        ///  .
        public float Width {
            get => m_Width;
        }

        ///  .
        public float LengthScale {
            get => m_LengthScale;
        }

        ///  .
        public uint Count {
            get => m_Count;
        }

        ///  .
        public bool IsEnabled {
            get => m_IsEnabled;
        }
    }

    readonly struct Ray {
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
    }
}

}