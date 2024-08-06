using System;
using System.Collections.Generic;
using Soil;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ThirdPerson {

public partial class DebugDraw {
    // the list of tags
    public static readonly Tag Default   = new(nameof(Default));
    public static readonly Tag None      = new(nameof(None));
    public static readonly Tag Collision = new(nameof(Collision));
    public static readonly Tag Surface   = new(nameof(Surface));
    public static readonly Tag Movement  = new(nameof(Movement));
    public static readonly Tag Friction  = new(nameof(Friction));
    public static readonly Tag Model     = new(nameof(Model));
    public static readonly Tag Walk      = new(nameof(Walk));
    public static readonly Tag Arms      = new(nameof(Arms));

    /// a debug value tag
    [Serializable]
    public record Tag {
        [Tooltip("the name of the tag")]
        public readonly string Name;

        [Tooltip("if the tag is currently enabled")]
        public bool IsEnabled;

        [Tooltip("the map of values")]
        [SerializeField] Map<string, Value> m_Values = new();

        /// create a new tag with a given name
        public Tag(string name) {
            Name = name;
            IsEnabled = false;
        }

        // -- commands --
        /// push a value for this tag
        public Value Push(
            string name,
            Color color = default,
            int count = BufferLen,
            float width = 1f,
            float scale = 1f,
            float minAlpha = 1f,
            bool force = false
        ) {
            // see if the value already exists
            var didExist = m_Values.TryGetValue(name, out var value);

            // if not, create one
            if (!didExist) {
                value = new Value();
                m_Values.Add(name, value);
                m_Values.Sort((l, r) => string.CompareOrdinal(l.Key, r.Key));
            }

            // if not, or if we want to update the config, do so
            if (!didExist || force) {
                value.Init(color, count, width, scale, minAlpha);
            }

            return value;
        }

        /// add an empty entry to the values
        public void Prepare() {
            foreach (var value in m_Values.Values) {
                value.Prepare();
            }
        }

        /// clear the values for this tag
        public void Clear() {
            foreach (var (_, value) in m_Values) {
                value.Clear();
            }
        }

        // -- queries --
        /// an enumerable of this tag's values
        public IEnumerable<Value> Values {
            get => m_Values.Values;
        }

        // -- debug --
        public override string ToString() {
            return Name;
        }
    }
}

}