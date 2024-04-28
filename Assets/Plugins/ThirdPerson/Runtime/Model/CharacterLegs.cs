using Soil;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Tooltip("the spring constant when blending the hips offset")]
    [SerializeField] float m_Hips_Spring;

    [Tooltip("the spring damping when blending the hips offset")]
    [SerializeField] float m_Hips_Damping;

    [Tooltip("the height of the skip")]
    [SerializeField] MapInCurve m_Hips_SkipOffset;

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

    /// the interpolated hips offset
    float m_Hips_CurrOffset;

    /// the current spring speed
    float m_Hips_Spring_Speed;

    /// the distance between the current and dest offset the previous frame
    float m_Hips_Spring_PrevDist;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Start() {
        m_InitialPos = transform.localPosition;
        m_InitialModelPos = m_Model.transform.localPosition;
    }

    void Update() {
        var delta = Time.deltaTime;

        // if the character is currently striding
        SetIsStriding(!c.State.Curr.IsCrouching && !c.State.Curr.IsInJumpSquat);

        // if we are not moving, make sure both legs are holding
        if (c.State.Curr.IsIdle) {
            Hold();
        }
        // if the held leg becomes free, release the moving leg
        else if (m_Left.IsFree != m_Right.IsFree) {
            Release();
        }
        // if both legs are held, start moving one
        else if (m_Left.IsHeld && m_Right.IsHeld) {
            Switch();
        }

        // slide the held leg if necessary
        Slide(delta);
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // add an offset to move the hips to match the character's stance
        MoveHips(delta);
    }

    // -- commands --
    /// update if the limbs are currently striding
    void SetIsStriding(bool isStriding) {
        m_Left.SetIsStriding(isStriding);
        m_Right.SetIsStriding(isStriding);
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

    /// switch the moving leg
    void Switch() {
        // move leg that is furthest away
        var (move, hold) = GetExtension(m_Left) > GetExtension(m_Right)
            ? (m_Left, m_Right)
            : (m_Right, m_Left);

        move.Move(hold);

        // draw hips & legs on move
        DebugDraw.PushLine(
            "legs-hips",
            m_Left.RootPos,
            m_Right.RootPos,
            new DebugDraw.Config(color: Color.yellow, DebugDraw.Tag.Model, count: 100)
        );

        m_Left.Debug_Draw("legs", count: 100);
        m_Right.Debug_Draw("legs", count: 100);
    }

    /// make sure both legs are free
    void Release() {
        m_Left.Release();
        m_Right.Release();
    }

    /// make sure both legs are holding
    void Hold() {
        m_Left.Hold();
        m_Right.Hold();
    }

    /// move hips according to current stride
    void MoveHips(float delta) {
        var hipsOffset = 0f;

        var heldLeg = m_Left.IsHeld ? m_Left : m_Right;
        if (heldLeg.IsHeld) {
            // move hips to correct for leg splay
            var srcCos = Vector3.Dot(heldLeg.InitialDir, Vector3.down);
            var curOffset = m_InitialPos - transform.localPosition;
            var curDir = Vector3.Normalize(heldLeg.GoalPos - heldLeg.RootPos - curOffset);
            var curCos = Vector3.Dot(curDir, Vector3.down);
            var curAngle = Mathf.Acos(curCos) * Mathf.Rad2Deg;

            // add the offset below skip threshold
            if (curAngle < m_Hips_SkipOffset.Src.Min) {
                // move hips to correct for distance from the bottom of the character to the current surface
                hipsOffset += heldLeg.HeldDistance;

                // move hips down according to leg splay
                hipsOffset += (srcCos - curCos) * heldLeg.InitialLen;
            }
            // curve offset above skip threshold
            else {
                var skipCos = Mathf.Cos(m_Hips_SkipOffset.Src.Min);
                var skipOffset = (srcCos - skipCos) * heldLeg.InitialLen;

                hipsOffset += Mathf.LerpUnclamped(
                    skipOffset,
                    0f,
                    m_Hips_SkipOffset.Evaluate(curAngle)
                );
            }
        }

        // TODO: extract spring damp struct
        // blend offset
        var currOffset = m_Hips_CurrOffset;
        var maxOffset = heldLeg.InitialLen;
        var destOffset = Mathf.Min(hipsOffset, maxOffset);

        var offsetDist = destOffset - currOffset;
        var offsetDistSpeed = (offsetDist - m_Hips_Spring_PrevDist) / delta;
        var nextSpeed = m_Hips_Spring_Speed + (m_Hips_Spring * offsetDist - m_Hips_Damping * offsetDistSpeed) * delta;
        var nextOffset = currOffset + nextSpeed * delta;

        m_Hips_CurrOffset = nextOffset;
        m_Hips_Spring_Speed = nextSpeed;
        m_Hips_Spring_PrevDist = offsetDist;

        // apply hip offset
        var translation = m_Hips_CurrOffset * Vector3.down;
        transform.localPosition = m_InitialPos + translation;
        m_Model.localPosition = m_InitialModelPos + translation;
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