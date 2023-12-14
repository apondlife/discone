using System;
using UnityEngine;
using System.Collections.Generic;
using Shapes;
using Soil;
using Random = UnityEngine.Random;

namespace  ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw: ImmediateModeShapeDrawer {
    // -- constants --
    /// the default buffer length
    const int k_BufferLen = 300;

    /// the debug draw key
    const KeyCode k_ToggleKey = KeyCode.Alpha0;

    /// the debug draw key
    const KeyCode k_ClearKey = KeyCode.Minus;

    /// the debug draw key
    const KeyCode k_PauseKey = KeyCode.Alpha9;

    /// the debug disable all key
    const KeyCode k_DisableAllKey = KeyCode.Equals;

    // -- static --
    /// the singleton instance
    static DebugDraw s_Instance;

    // -- fields --
    [Header("fields")]
    [Tooltip("if drawing is enabled")]
    [SerializeField] bool m_IsEnabled;

    [Tooltip("if value collection is paused")]
    [SerializeField] bool m_IsPaused;

    [Tooltip("the index range of values to draw")]
    [Soil.Range(0, k_BufferLen)]
    [SerializeField] IntRange m_Range = new(0, k_BufferLen);

    // TODO: make this a keyable type
    [Tooltip("the map of values")]
    [SerializeField] List<Value> m_Values = new();

    // -- lifecycle --
    void Awake() {
        s_Instance = this;
    }

    void Update() {
        // toggle drawing on press
        if (Input.GetKeyDown(k_ToggleKey)) {
            m_IsEnabled = !m_IsEnabled;
        }

        // toggle value collection on press
        if (Input.GetKeyDown(k_PauseKey)) {
            m_IsPaused = !m_IsPaused;
        }

        // clear on press
        if (Input.GetKeyDown(k_ClearKey)) {
            foreach (var value in m_Values) {
                value.Clear();
            }
        }

        // disable all on press
        if (Input.GetKeyDown(k_DisableAllKey)) {
            foreach (var value in m_Values) {
                value.IsEnabled = false;
            }
        }
    }

    // -- commands --
    /// push the drawing's next value
    public static void Push(string name, Vector3 pos, Vector3 dir) {
        var initialValue = new Value(
            name,
            color: Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f),
            width: 1f,
            lengthScale: 1f,
            minAlpha: 1f
        );

        Push(initialValue, pos, dir);
    }

    /// push the drawing's current value
    public static void Push(Value initialValue, Vector3 pos, Vector3 dir) {
        s_Instance.PushNext(initialValue, pos, dir);
    }

    /// push the drawing's current value
    void PushNext(Value initialValue, Vector3 pos, Vector3 dir) {
        // don't push when not enabled
        if (!m_IsEnabled || m_IsPaused) {
            return;
        }

        // find or create the value
        // TODO: convert this to serializable dictionary-like storage
        var value = null as Value;
        foreach (var other in m_Values) {
            if (other.Name == initialValue.Name) {
                value = other;
                break;
            }
        }

        // if new, add it in sorted order
        if (value == null) {
            value = initialValue;
            m_Values.Add(value);
            m_Values.Sort((l, r) => string.CompareOrdinal(l.Name, r.Name));
        }

        // push the current value
        value.Push(new Ray(pos, dir));
    }

    // -- data --
    /// a value that is buffered over n frames
    [Serializable]
    public record Value {
        /// the name of this value
        [HideInInspector]
        [SerializeField] string m_Name;

        // -- fields --
        [Tooltip("the rendered color")]
        [SerializeField] Color m_Color;

        [Tooltip("the index range of values to draw")]
        [Soil.Range(0, k_BufferLen)]
        [SerializeField] IntRange m_Range = new(0, k_BufferLen);

        [Tooltip("the line width")]
        [SerializeField] float m_Width;

        [Tooltip("the length scale")]
        [SerializeField] float m_LengthScale;

        [Tooltip("the min alpha as the value fades out")]
        [UnityEngine.Range(0f, 1f)]
        [SerializeField] float m_MinAlpha;

        [Tooltip("if the value is visible")]
        [SerializeField] bool m_IsEnabled;

        // -- props --
        /// the buffer of values
        Queue<Ray> m_Buffer;

        // -- lifetime --
        public Value(
            string name,
            Color color,
            int count = k_BufferLen,
            float width = 1f,
            float lengthScale = 1f,
            float minAlpha = 1f
        ) {
            m_Name = name;
            m_Color = color;
            m_Range = new IntRange(0, count);
            m_Width = width;
            m_LengthScale = lengthScale;
            m_MinAlpha = minAlpha;
            m_IsEnabled = true;
            m_Buffer = new Queue<Ray>(k_BufferLen);
        }

        // -- commands --
        /// .
        public void Push(Ray next) {
            m_Buffer.Add(next);
        }

        /// .
        public void Clear() {
            m_Buffer.Clear();
        }

        // -- queries --
        /// gets the nth-newest value
        public Ray this[int offset] {
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
        public IntRange Range {
            get => m_Range;
        }

        ///  .
        public float MinAlpha {
            get => m_MinAlpha;
        }

        ///  .
        public bool IsEnabled {
            get => m_IsEnabled;
            set => m_IsEnabled = value;
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
    }
}

}