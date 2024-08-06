using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shapes;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a debug utility for adding drawings
public partial class DebugDraw: ImmediateModeShapeDrawer {
    // -- constants --
    /// the default buffer length
    public const int BufferLen = 300;

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

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the set of tags to show")]
    [SerializeField] Tag[] m_Tags;

    [Tooltip("if drawing is on")]
    [SerializeField] bool m_IsDrawing;

    [Tooltip("if value collection is paused")]
    [SerializeField] bool m_IsPaused;

    [Tooltip("the index range of values to draw")]
    [Soil.Range(0, BufferLen)]
    [SerializeField] IntRange m_Range = new(0, BufferLen);

    // -- props --
    /// the last color's hue
    float m_Hue;

    /// the index of the last selected tag
    int m_TagIndex;

    // -- lifecycle --
    void Awake() {
        // store the instance
        s_Instance = this;

        // grab every tag
        var tags = typeof(DebugDraw)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where((field) => field.FieldType == typeof(Tag))
            .Select((field) => field.GetValue(null))
            .Cast<Tag>()
            .ToArray();

        // and store it for cycling / toggling
        m_Tags = tags;
    }

    void Update() {
        // toggle drawing on press
        if (Input.GetKeyDown(k_ToggleKey)) {
            m_IsDrawing = !m_IsDrawing;
        }

        // toggle value collection on press
        if (Input.GetKeyDown(k_PauseKey)) {
            m_IsPaused = !m_IsPaused;
        }

        // clear on press
        if (Input.GetKeyDown(k_ClearKey)) {
            foreach (var tag in m_Tags) {
                tag.Clear();
            }
        }

        // cycle tags all on press
        if (Input.GetKeyDown(k_CycleTagsKey)) {
            var tagCount = m_Tags.Length;
            var tagIndex = (m_TagIndex + 1) % tagCount;

            // enable only the selected tag
            for (var i = 0; i < tagCount; i++) {
                var tag = m_Tags[i];
                tag.IsEnabled = i == tagIndex;
            }

            // update state
            m_TagIndex = tagIndex;
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
            foreach (var tag in m_Tags) {
                tag.Prepare();
            }
        }
    }

    // -- queries --
    /// if drawing is enabled
    public bool IsEnabled {
        get => m_IsDrawing;
    }

    /// the available tags
    public IEnumerable<Tag> Tags {
        get => m_Tags;
    }
}

}