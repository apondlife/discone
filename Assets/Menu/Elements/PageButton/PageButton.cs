using System;
using System.Linq;
using Soil;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// a page button that randomizes its orientation
sealed class PageButton: UIBehaviour {
    // -- types --
    /// an orientation for the button
    [Flags]
    enum Orientation {
        Up    = 1 << 0,
        Down  = 1 << 1,
        Left  = 1 << 2,
        Right = 1 << 3
    }

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the set of orientations this button picks from")]
    [SerializeField] Orientation m_Orientations;

    [Tooltip("the set of orientations this button picks from")]
    [SerializeField] MapOutCurve m_CrossAxisRange;

    // -- commands --
    /// change the button's orientation
    void ChangeOrientation() {
        var r = transform as RectTransform;

        // pick an orientation
        var orientation = EnumExt
            .Enumerable<Orientation>()
            .Where((c) => (m_Orientations & c) == c)
            .Sample();

        var s = r.sizeDelta;
        var x = m_CrossAxisRange.Evaluate(UnityEngine.Random.value);
        var o = orientation switch {
            Orientation.Up => new OrientationProps(
                pos: new Vector2(x, -30f - s.y),
                rot: 180f,
                anc: new Vector2(0.5f, 1.0f)
            ),
            Orientation.Down => new OrientationProps(
                pos: new Vector2(x, 30f),
                rot: 0f,
                anc: new Vector2(0.5f, 0.0f)
            ),
            Orientation.Left => new OrientationProps(
                pos: new Vector2(30f + s.x * 0.5f, x),
                rot: 270f,
                anc: new Vector2(0.0f, 0.5f)
            ),
            /*Orientation.Right*/ _ => new OrientationProps(
                pos: new Vector2(-30f - s.x * 0.5f, x),
                rot: 90f,
                anc: new Vector2(1.0f, 0.5f)
            ),
        };

        // reposition element
        r.pivot = o.Anchor;
        r.anchorMin = o.Anchor;
        r.anchorMax = o.Anchor;
        r.rotation = Quaternion.Euler(0f, 0f, o.Rot);
        r.anchoredPosition = o.Pos;
    }

    // -- events --
    /// when an element is about to enter the screen
    public void OnBeforeEnter() {
        ChangeOrientation();
    }

    // -- data types --
    /// a set of orientation props for configuring the button
    readonly struct OrientationProps {
        // -- props --
        /// the anchored position
        public readonly Vector2 Pos;

        /// the rotation in degrees
        public readonly float Rot;

        /// the container anchor
        public readonly Vector2 Anchor;

        // -- lifetime --
        /// create an orienation
        public OrientationProps(Vector2 pos, float rot, Vector2 anc) {
            Pos = pos;
            Rot = rot;
            Anchor = anc;
        }
    }

}

}