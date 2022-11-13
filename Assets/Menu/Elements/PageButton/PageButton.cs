using System;
using System.Linq;
using UnityEngine;

namespace Discone.Ui {

/// a page button that randomizes its orientation
[RequireComponent(typeof(MenuElement))]
sealed class PageButton: MonoBehaviour {
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
    [SerializeField] ThirdPerson.RangeCurve m_CrossAxisRange;

    // -- refs --
    [Header("refs")]
    [Tooltip("the button rect")]
    [SerializeField] RectTransform m_Root;

    // -- props --
    /// this button's menu element
    MenuElement m_Element;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Element = GetComponent<MenuElement>();
    }

    // -- commands --
    /// change the button's orientation
    void ChangeOrientation() {
        var orientation = EnumExt
            .Enumerable<Orientation>()
            .Where((c) => (m_Orientations & c) == c)
            .Sample();

        // pick an orientation
        var s = m_Root.sizeDelta;
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
        var r = m_Root;
        r.pivot = o.Anchor;
        r.anchorMin = o.Anchor;
        r.anchorMax = o.Anchor;
        r.rotation = Quaternion.Euler(0f, 0f, o.Rot);
        r.anchoredPosition = o.Pos;

        // invalidate initial position
        m_Element.InvalidatePosition();
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
