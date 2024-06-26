using System;
using UnityEngine;

namespace Soil {

[Serializable]
public struct AnyCurve: ISerializationCallbackReceiver {
    public enum Variant {
        Map,
        MapIn,
        MapOut,
        Animation
    }

    [Tooltip("the type of the the curve")]
    public Variant Type;

    [NonSerialized]
    [Tooltip("the curve")]
    public FloatTransform Curve;

    // -- ISerializationCallbackReceiver --
    [Serializable]
    public struct Serialized {
        public FloatRange Src;
        public FloatRange Dst;
        public AnimationCurve Curve;
    }

    [HideInInspector]
    [SerializeField] Serialized m_Serialized;

    public void OnBeforeSerialize() {
        switch (Curve) {
        case MapCurve c:
            m_Serialized.Src = c.Src;
            m_Serialized.Dst = c.Dst;
            m_Serialized.Curve = c.Curve;
            break;
        case MapInCurve c:
            m_Serialized.Src = c.Src;
            m_Serialized.Curve = c.Curve;
            break;
        case MapOutCurve c:
            m_Serialized.Dst = c.Dst;
            m_Serialized.Curve = c.Curve;
            break;
        case AnimationCurveBox c:
            m_Serialized.Curve = c.Inner;
            break;
        }
    }

    public void OnAfterDeserialize() {
        Curve = Type switch {
            Variant.Map => new MapCurve() {
                Src = m_Serialized.Src,
                Dst = m_Serialized.Dst,
                Curve = m_Serialized.Curve,
            },
            Variant.MapIn => new MapInCurve() {
                Src = m_Serialized.Src,
                Curve = m_Serialized.Curve,
            },
            Variant.MapOut => new MapOutCurve() {
                Dst = m_Serialized.Dst,
                Curve = m_Serialized.Curve,
            },
            Variant.Animation => new AnimationCurveBox() {
                Inner = m_Serialized.Curve,
            },
            _ => Unsupported()
        };
    }

    FloatTransform Unsupported() {
        throw new NotSupportedException($"no deserialization for {Type}");
    }
}

}