using Soil;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;

namespace ThirdPerson {

/// a pair of legs working in unison
class CharacterLegs: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the left leg")]
    [SerializeField] Limb m_Left;

    [Tooltip("the right leg")]
    [SerializeField] Limb m_Right;

    // -- tuning --
    [Header("tuning")]
    [FormerlySerializedAs("m_SlideThreshold")]
    [Tooltip("the threshold under which the character legs slide opposite movement")]
    [SerializeField] float m_Slide_Threshold;

    [FormerlySerializedAs("m_SlideSpeed")]
    [Tooltip("the speed the legs slide")]
    [SerializeField] float m_Slide_Speed;

    [Tooltip("the minimum & maximum offset downwards of the hips as a scale on limb length (min is no offset)")]
    [SerializeField] FloatRange m_Hips_OffsetRangeScale;

    [Tooltip("the height of the skip")]
    [SerializeField] MapInCurve m_Hips_SkipOffset;

    [Tooltip("the hips offset ease")]
    [SerializeField] DynamicEase m_Hips_Ease;

    // -- refs --
    [Header("refs")]
    [Tooltip("the attached model")]
    [SerializeField] Transform m_Model;

    // -- props --
    /// the character's dependency container
    CharacterContainer c;

    /// the initial position of the leg
    Vector3 m_InitialPos;

    /// the initial position of the model
    Vector3 m_InitialModelPos;

    /// the previous world position
    Vector3 m_Debug_PrevInitialPos;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Start() {
        m_InitialPos = transform.localPosition;
        m_InitialModelPos = m_Model.transform.localPosition;
        m_Hips_Ease.Init(Vector3.zero);
    }

    void Update() {
        var delta = Time.deltaTime;

        // anchor the legs to one another
        m_Left.State.Anchor = m_Right.IntoAnchor();
        m_Right.State.Anchor = m_Left.IntoAnchor();

        // if the character is currently striding
        SetIsStriding(!c.State.Curr.IsCrouching && !c.State.Curr.IsInJumpSquat);

        // if we are not moving and legs are closer than minimum, make sure both legs are holding
        var totalExtension = Mathf.Abs(GetExtension(m_Left) + GetExtension(m_Right));
        if (c.State.Curr.IsIdle && totalExtension < 2.0f * m_Left.Tuning.MaxLength.Min) {
            Hold(delta);
        }
        // if the held leg becomes free, release the moving leg
        else if (m_Left.State.IsFree != m_Right.State.IsFree) {
            Release();
        }
        // if both legs are held, start moving one
        else if (m_Left.State.IsHeld && m_Right.State.IsHeld) {
            Switch();
        }

        // slide the held leg if necessary
        Slide(delta);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // add an offset to move the hips to match the character's stance
        OffsetHips(delta);
    }

    // -- commands --
    /// update if the limbs are currently striding
    void SetIsStriding(bool isStriding) {
        m_Left.SetIsStriding(isStriding);
        m_Right.SetIsStriding(isStriding);
    }

    /// make sure both legs are holding
    void Hold(float delta) {
        m_Left.Hold(delta);
        m_Right.Hold(delta);
    }

    /// make sure both legs are free
    void Release() {
        m_Left.Release();
        m_Right.Release();
    }

    /// switch the moving leg
    void Switch() {
        // move leg that is furthest away
        var move = GetExtension(m_Left) > GetExtension(m_Right) ? m_Left : m_Right;
        move.Move();

        // draw hips & legs on move
        DebugDraw.PushLine(
            "legs-hips",
            m_Left.RootPos,
            m_Right.RootPos,
            new DebugDraw.Config(color: Color.yellow, DebugDraw.Tag.Walk, count: 100)
        );

        m_Left.Debug_Draw("legs", count: 100);
        m_Right.Debug_Draw("legs", count: 100);
    }

    /// slide the held leg when they lose grip
    void Slide(float delta) {
        // add an offset to slide the legs when they lose grip
        var slideOffset = Vector3.zero;

        // set offset in move direction
        var v = c.State.Curr.SurfaceVelocity;
        var moveDir = v != Vector3.zero ? v.normalized : c.State.Curr.Forward;
        var moveInput = c.Inputs.MoveMagnitude;

        var isHeldLegSliding = (
            // if we have input
            moveInput > 0f &&
            // and the velocity towards input is below a threshold
            Vector3.Dot(v, c.Inputs.Move.normalized) < m_Slide_Threshold
        );

        if (isHeldLegSliding) {
            slideOffset = moveInput * m_Slide_Speed * delta * moveDir;
        }

        m_Left.SetSlideOffset(slideOffset);
        m_Right.SetSlideOffset(slideOffset);
    }

    /// offset hips downwards according to current stride
    void OffsetHips(float delta) {
        var hipsOffset = Vector3.zero;

        var heldLeg = m_Left.State.HeldDistance < m_Right.State.HeldDistance ? m_Left : m_Right;
        if (heldLeg.State.IsHeld) {
            // get the angle of the current stride, as if there was no offset
            var curOffset = m_InitialPos - transform.localPosition;
            var curDir = Vector3.Normalize(heldLeg.GoalPos - heldLeg.RootPos - curOffset);
            var curCos = Vector3.Dot(curDir, Vector3.down);
            var curAngle = Mathf.Acos(curCos) * Mathf.Rad2Deg;

            // add the offset below skip threshold
            if (curAngle < m_Hips_SkipOffset.Src.Min) {
                // offset hips to account for distance to surface
                hipsOffset += m_Hips_Ease.Value + (heldLeg.State.HeldDistance * Vector3.down);
            }
            // curve offset above skip threshold
            else {
                var srcCos = Vector3.Dot(heldLeg.SearchDir, Vector3.down);
                var skipCos = Mathf.Cos(m_Hips_SkipOffset.Src.Min);
                var skipOffset = (srcCos - skipCos) * heldLeg.InitialLen;

                hipsOffset += Vector3.down * Mathf.LerpUnclamped(
                    skipOffset,
                    0f,
                    m_Hips_SkipOffset.Evaluate(curAngle)
                );
            }
        }

        // clamp offset within vertical limits
        var offsetRange = m_Hips_OffsetRangeScale * heldLeg.InitialLen;
        hipsOffset.y = offsetRange.ClampMagnitude(hipsOffset.y);

        // ease the target offset
        var prevOffset = m_Hips_Ease.Value;
        var prevTarget = m_Hips_Ease.Target;
        m_Hips_Ease.Update(delta, hipsOffset);

        // apply hip offset
        var t = transform;
        var translation = m_Hips_Ease.Value;
        t.localPosition = m_InitialPos + translation;
        m_Model.localPosition = m_InitialModelPos + translation;

        var pos = t.parent.TransformPoint(m_InitialPos);
        DebugDraw.PushLine("legs-hips-pos", m_Debug_PrevInitialPos + prevOffset, pos + m_Hips_Ease.Value, new DebugDraw.Config(Soil.Color.GreenYellow, width: 1f));
        DebugDraw.PushLine("legs-hips-target", m_Debug_PrevInitialPos + prevTarget, pos + m_Hips_Ease.Target, new DebugDraw.Config(Soil.Color.MediumVioletRed, width: 1f));
        m_Debug_PrevInitialPos = pos;
    }

    // -- queries --
    /// the displacement of the leg projected along the move dir
    float GetExtension(Limb limb) {
        // TODO: how many places are we implementing a fallback to forward in some fashion?
        var moveDir = c.State.Curr.PlanarVelocity;
        if (moveDir == Vector3.zero) {
            moveDir = c.State.Curr.Forward;
        }

        var displacement = Vector3.Dot(
            limb.GoalPos - limb.RootPos,
            moveDir
        );

        return -displacement;
    }
}

}