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

    /// the maximum frame window length on resize
    const int k_WindowMax = 2;

    /// the toggle drawing key
    const KeyCode k_ToggleKey = KeyCode.Alpha9;

    /// the pause drawing key
    const KeyCode k_PauseKey = KeyCode.Alpha0;

    /// the clear drawings key
    const KeyCode k_ClearKey = KeyCode.Minus;

    /// the disable all values key
    const KeyCode k_DisableAllKey = KeyCode.Equals;

    /// the frame advance key
    const KeyCode k_Advance = KeyCode.RightBracket;

    /// the frame rewind key
    const KeyCode k_Rewind = KeyCode.LeftBracket;

    /// the key to resize the frame window
    const KeyCode k_ResizeWindow = KeyCode.Backslash;

    // -- statics --
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

        // advance on press
        if (Input.GetKeyDown(k_Advance)) {
            m_Range.Min -= 1;
            m_Range.Max -= 1;
        }

        // rewind on press
        if (Input.GetKeyDown(k_Rewind)) {
            m_Range.Min += 1;
            m_Range.Max += 1;
        }

        // resize frame window on press
        if (Input.GetKeyDown(k_ResizeWindow)) {
            var currLen = m_Range.Max - m_Range.Min;
            var nextLen = currLen >= k_WindowMax ? 1 : currLen + 1;
            m_Range.Max = m_Range.Min + nextLen;
        }
    }

    void FixedUpdate() {
        // push an empty frame for each existing value
        if (!m_IsPaused) {
            foreach (var value in m_Values) {
                value.Push(new Ray());
            }
        }
    }

    // -- commands --
    /// push the drawing's next value
    public static void Push(string name, Vector3 pos, Vector3 dir) {
        var cfg = new Config(color: Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f));
        Push(name, pos, dir, cfg);
    }

    /// push the drawing's next value
    public static void Push(string name, Vector3 pos, Vector3 dir, Config cfg) {
        var initialValue = new Value(name, cfg);
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

        // the ray to add
        var ray = new Ray(pos, dir);

        // if it exists, update value pushed at the beginning of the frame
        if (value != null) {
            value.Set(ray);
        }
        // if new, add it in sorted order
        else {
            value = initialValue;
            m_Values.Add(value);
            m_Values.Sort((l, r) => string.CompareOrdinal(l.Name, r.Name));

            // and push the initial value
            value.Push(ray);
        }
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
        [SerializeField] Gradient m_Color;

        [Tooltip("the index range of values to draw")]
        [Soil.Range(0, k_BufferLen)]
        [SerializeField] IntRange m_Range = new(0, k_BufferLen);

        [Tooltip("the line width")]
        [SerializeField] float m_Width;

        [Tooltip("the length scale")]
        [SerializeField] float m_Scale;

        [Tooltip("if the value is visible")]
        [SerializeField] bool m_IsEnabled;

        // -- props --
        /// the buffer of values
        Queue<Ray> m_Buffer;

        // -- lifetime --
        public Value(
            string name,
            Config cfg
        ) {
            var gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[] { new(cfg.Color, 0f) };
            gradient.alphaKeys = new GradientAlphaKey[] { new(cfg.MinAlpha, 0f), new (1f, 1f) };

            m_Name = name;
            m_Color = gradient;
            m_Range = new IntRange(0, cfg.Count);
            m_Width = cfg.Width;
            m_Scale = cfg.Scale;
            m_IsEnabled = true;
            m_Buffer = new Queue<Ray>(k_BufferLen);
        }

        // -- commands --
        /// push a new frame
        public void Push(Ray next) {
            m_Buffer.Add(next);
        }

        /// set ray for the current frame
        public void Set(Ray next) {
            m_Buffer[0] = next;
        }

        /// clear the value
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

    /// the cosmetic config for a value
    public readonly struct Config {
        // -- props --
        /// the rendered color
        public readonly Color Color;

        /// the index range of values to draw
        public readonly int Count;

        /// the line width
        public readonly float Width;

        /// the length scale
        public readonly float Scale;

        /// the min alpha
        public readonly float MinAlpha;

        // -- lifetime --
        public Config(
            Color color,
            int count = k_BufferLen,
            float width = 1f,
            float scale = 1f,
            float minAlpha = 1f
        ) {
            Color = color;
            Count = count;
            Width = width;
            Scale = scale;
            MinAlpha = minAlpha;
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