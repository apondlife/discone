using Soil;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPerson {

/// a pair of legs working in unison
class CharacterLegs: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the left leg")]
    [SerializeField] CharacterLimb m_Left;

    [Tooltip("the right leg")]
    [SerializeField] CharacterLimb m_Right;

    // -- tuning --
    [Header("tuning")]
    [FormerlySerializedAs("m_SlideThreshold")]
    [Tooltip("the threshold under which the character legs slide opposite movement")]
    [SerializeField] float m_Slide_Threshold;

    [FormerlySerializedAs("m_SlideSpeed")]
    [Tooltip("the speed the legs slide")]
    [SerializeField] float m_Slide_Speed;

    [Tooltip("the blending speed for the model")]
    [SerializeField] float m_Hips_BlendSpeed;

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
    float m_Hips_ModelOffset;

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


        // if the character is currently striding
        SetIsStriding(!c.State.Curr.IsCrouching && !c.State.Curr.IsInJumpSquat);

        // if the held leg becomes free, release the moving leg
        if (m_Left.IsFree != m_Right.IsFree) {
            Release();
        }
        // if both legs are held, start moving one
        else if (m_Left.IsHeld && m_Right.IsHeld) {
            Switch();
        }

        // add an offset to move the hips to match the character's stance
        var hipsOffset = 0f;

        var heldLeg = m_Left.IsHeld ? m_Left : m_Right;
        if (heldLeg.IsHeld) {
            var srcCos = Vector3.Dot(heldLeg.InitialDir, Vector3.down);
            var curOffset = m_InitialPos - transform.localPosition;
            var curDir = Vector3.Normalize(heldLeg.GoalPos - heldLeg.RootPos - curOffset);
            var curCos = Vector3.Dot(curDir, Vector3.down);
            var curAngle = Mathf.Acos(curCos) * Mathf.Rad2Deg;

            if (curAngle < m_Hips_SkipOffset.Src.Min) {
                hipsOffset = (srcCos - curCos) * heldLeg.InitialLen;
            } else {
                var skipCos = Mathf.Cos(m_Hips_SkipOffset.Src.Min);
                var skipOffset = (srcCos - skipCos) * heldLeg.InitialLen;

                hipsOffset = Mathf.LerpUnclamped(
                    skipOffset,
                    0f,
                    m_Hips_SkipOffset.Evaluate(curAngle)
                );
            }
        }

        // blend offset
        m_Hips_ModelOffset = Mathf.MoveTowards(
            m_Hips_ModelOffset,
            hipsOffset,
            m_Hips_BlendSpeed * delta
        );

        // apply hip offset
        transform.localPosition = m_InitialPos + hipsOffset * Vector3.down;
        m_Model.localPosition = m_InitialModelPos + m_Hips_ModelOffset * Vector3.down;
    }

    // -- commands --
    /// update if the limbs are currently striding
    void SetIsStriding(bool isStriding) {
        m_Left.SetIsStriding(isStriding);
        m_Right.SetIsStriding(isStriding);
    }

    /// switch the moving leg
    void Switch() {
        // move leg that is furthest away
        var (move, hold) = m_Left.SqrLength > m_Right.SqrLength
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
}

}