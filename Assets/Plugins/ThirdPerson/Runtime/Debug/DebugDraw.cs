using System;
using System.Collections.Generic;
using Shapes;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw: ImmediateModeShapeDrawer {
    // -- tags --
    [Flags]
    public enum Tag {
        Default   = 1 << 0,
        None      = 1 << 1,
        Collision = 1 << 2,
        Surface   = 1 << 3,
        Movement  = 1 << 4,
        Friction  = 1 << 5,
        Model     = 1 << 6,
        Walk      = 1 << 7,
    }

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
    const KeyCode k_CycleTagsKey = KeyCode.Equals;

    /// the frame advance key
    const KeyCode k_Advance = KeyCode.RightBracket;

    /// the frame rewind key
    const KeyCode k_Rewind = KeyCode.LeftBracket;

    /// the key to resize the frame window
    const KeyCode k_ResizeWindow = KeyCode.Backslash;

    // -- statics --
    /// the singleton instance
    static DebugDraw s_Instance;

    /// the last tag
    static Tag s_LastTag;

    /// initialize statics
    static DebugDraw() {
        var tags = Enum.GetValues(typeof(Tag));
        s_LastTag = (Tag)tags.GetValue(tags.Length - 1);
    }

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the set of tags to show")]
    [SerializeField] Tag m_Tags = Tag.Default;

    [Tooltip("if drawing is enabled")]
    [SerializeField] bool m_IsEnabled;

    [Tooltip("if value collection is paused")]
    [SerializeField] bool m_IsPaused;

    [Tooltip("the index range of values to draw")]
    [Soil.Range(0, k_BufferLen)]
    [SerializeField] IntRange m_Range = new(0, k_BufferLen);

    [Tooltip("the map of values")]
    [SerializeField] Map<string, Value> m_Values = new();

    // -- props --
    /// the last color's hue
    float m_Hue;

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
            foreach (var (_, value) in m_Values) {
                value.Clear();
            }
        }

        // cycle tags all on press
        if (Input.GetKeyDown(k_CycleTagsKey)) {
            var next = (int)m_Tags;
            if (next == 0 || (next & (next - 1)) != 0) {
                next = 1;
            } else if ((Tag)next == s_LastTag) {
                next = int.MaxValue;
            } else {
                next <<= 1;
            }

            m_Tags = (Tag)next;
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
            foreach (var (_, value) in m_Values) {
                value.Push(new Ray());
            }
        }
    }

    // -- commands --
    /// push the drawing's next point
    public static void Push(string name, Vector3 pos) {
        Push(name, pos, Vector3.zero);
    }

    /// push the drawing's next line
    public static void PushLine(string name, Vector3 src, Vector3 dst, Config cfg) {
        Push(name, src, dst-src, cfg);
    }

    /// push the drawing's next point
    public static void Push(string name, Vector3 pos, Config cfg) {
        Push(name, pos, Vector3.zero, cfg);
    }

    /// push the drawing's next ray
    public static void Push(string name, Vector3 pos, Vector3 dir) {
        var cfg = new Config(Color.clear);
        Push(name, pos, dir, cfg);
    }

    /// push the drawing's next ray
    public static void Push(string name, Vector3 pos, Vector3 dir, Config cfg) {
        if (!s_Instance) {
            return;
        }

        s_Instance.PushNext(name, pos, dir, cfg);
    }

    /// push the drawing's current value
    void PushNext(string name, Vector3 pos, Vector3 dir, Config cfg) {
        // don't push when not enabled
        if (!m_IsEnabled || m_IsPaused) {
            return;
        }

        // the ray to add
        var ray = new Ray(pos, dir);

        // if the value exists, update the ray pushed at the beginning of the frame
        m_Values.TryGetValue(name, out var value);
        if (value != null) {
            value.Set(ray);
        }
        // if new, add it in sorted order
        else {
            // pick a random color if default
            if (cfg.Color == Color.clear) {
                RotateColor();
                cfg = cfg.WithColor(CurrColor());
            }

            // add the value
            value = new Value(cfg);
            m_Values.Add(name, value);
            m_Values.Sort((l, r) => string.CompareOrdinal(l.Key, r.Key));

            // and push the initial ray
            value.Push(ray);
        }
    }

    // -- c/color
    /// rotate to the next color
    void RotateColor() {
        m_Hue = (m_Hue + 0.12f) % 1f;
    }

    // -- queries --
    /// get the current color
    Color CurrColor() {
        return Color.HSVToRGB(m_Hue, 1f, 1f);
    }

    // -- data --
    /// a value that is buffered over n frames
    [Serializable]
    public record Value {
        // -- fields --
        [Tooltip("this tags this value is associated to")]
        [SerializeField] Tag m_Tags;

        [Tooltip("the rendered color")]
        [SerializeField] Gradient m_Color;

        [Tooltip("the index range of values to draw")]
        [Soil.Range(0, k_BufferLen)]
        [SerializeField] IntRange m_Range = new(0, k_BufferLen);

        [Tooltip("the line width")]
        [SerializeField] float m_Width;

        [Tooltip("the length scale")]
        [SerializeField] float m_Scale;

        [Tooltip("if the value is enabled")]
        [SerializeField] bool m_IsEnabled;

        // -- props --
        /// the buffer of values
        Ring<Ray> m_Buffer;

        // -- lifetime --
        public Value(Config cfg) {
            var gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[] { new(cfg.Color, 0f) };
            gradient.alphaKeys = new GradientAlphaKey[] { new(cfg.MinAlpha, 0f), new (1f, 1f) };

            m_Tags = cfg.Tags;
            m_Color = gradient;
            m_Range = new IntRange(0, cfg.Count);
            m_Width = cfg.Width;
            m_Scale = cfg.Scale;
            m_IsEnabled = true;
            m_Buffer = new Ring<Ray>(k_BufferLen);
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
        /// if the enabled and in the set of tags
        public bool IsVisible(Tag tags) {
            return m_IsEnabled && (m_Tags & tags) != 0;
        }

        /// gets the nth-newest value
        public Ray this[int offset] {
            get => m_Buffer[offset];
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
        /// the tag groups
        public readonly Tag Tags;

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
            Tag tags = Tag.None,
            int count = k_BufferLen,
            float width = 1f,
            float scale = 1f,
            float minAlpha = 1f
        ) {
            Tags = tags;
            Color = Color.clear;
            Count = count;
            Width = width;
            Scale = scale;
            MinAlpha = minAlpha;
        }

        public Config(
            Color color,
            Tag tags = Tag.None,
            int count = k_BufferLen,
            float width = 1f,
            float scale = 1f,
            float minAlpha = 1f
        ) {
            Tags = tags;
            Color = color;
            Count = count;
            Width = width;
            Scale = scale;
            MinAlpha = minAlpha;
        }

        // -- operators --
        /// create a copy of the config w/ the color
        public Config WithColor(Color color) {
            return new Config(color, Tags, Count, Width, Scale, MinAlpha);
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