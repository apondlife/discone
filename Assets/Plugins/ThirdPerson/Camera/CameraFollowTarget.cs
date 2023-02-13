using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- cfg --
    [Tooltip("the tuning parameters for the camera")]
    [SerializeField] CameraTuning m_Tuning;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the output follow destination for cinemachine")]
    [SerializeField] Transform m_Destination;

    [Tooltip("the free look camera input")]
    [FormerlySerializedAs("m_FreeLook")]
    [SerializeField] InputActionReference m_Input;

    // -- props --
    /// the current camera state
    CameraState m_State;

    /// the current character state
    CharacterState m_CharacterState;

    /// storage for raycasts
    RaycastHit m_Hit;

    /// target damping velocity
    Vector3 m_CorrectionVel;

    /// the camera following sytem (state machine)
    CameraFollowSystem m_FollowSystem;

    // -- lifecycle --
    void Awake() {
        // set props
        m_State = new CameraState(new CameraState.Frame());
    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_CharacterState = character.State;

        // init systems
        m_FollowSystem = new CameraFollowSystem(
            m_State,
            m_Tuning,
            m_Input.action,
            m_CharacterState,
            transform.localPosition
        );

        m_FollowSystem.Init();

        // set initial position
        m_Destination.position = m_State.Pos;
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // snapshot state w/ current world position
        m_State.Next.Pos = m_Destination.position;
        m_State.Snapshot();

        // run systems
        m_FollowSystem.Update(delta);

        // run collision system
        m_State.Next.Pos = Vector3.SmoothDamp(
            m_State.Next.Pos,
            GetCorrectedPos(m_State.Next.Pos),
            ref m_CorrectionVel,
            m_Tuning.CorrectionSmoothTime,
            m_Tuning.CorrectionSpeed
        );

        // TODO:
        // m_State.Next.Spherical = IntoSpherical(m_State.Next.Pos);

        // update camera pos
        m_Destination.position = m_State.Next.Pos;
    }

    // -- queries --
    /// correct camera position in attempt to preserve line of sight
    /// see: https://miro.com/app/board/uXjVOWfpI6I=/?moveToWidget=3458764535240497690&cot=14
    private Vector3 GetCorrectedPos(Vector3 candidate) {
        // the final position
        var destPos = candidate;

        // the character's position
        var origin = transform.position;

        // step 1: cast from the character to the ideal position to see if any
        // surface is blocking visibility; use a sphere cast so we don't get
        // closer than the contact offset
        var vizCastStart = candidate - origin;
        var vizLen = vizCastStart.magnitude;
        var vizDir = vizCastStart.normalized;

        var didHit = Physics.SphereCast(
            origin,
            m_Tuning.ContactOffset,
            vizDir,
            out m_Hit,
            vizLen,
            m_Tuning.CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        // if the target is visible, we have our desired position
        if (!didHit) {
            return destPos;
        }

        // otherwise, we found the point on this surface (note: offset by c.o.
        // so that step 3b works)
        var vizNormal = m_Hit.normal;
        var vizPos = OffsetHit(m_Hit);

        destPos = vizPos;

        // step 2: project the candidate along the plane of the hit surface
        // using the remaining distance to candidate

        // scale the projection down if the pitch is < 0 so that we can pan
        // into the character
        var projK = 1.0f;
        var pitch = m_State.Next.Spherical.Zenith;
        if (pitch < 0.0f) {
            projK = 1.0f - pitch / m_Tuning.FreeLook_MinPitch;
        }

        var projLen = Vector3.Distance(candidate, vizPos);
        var projDir = Vector3.Cross(vizNormal, Vector3.Cross(vizDir, vizNormal)).normalized;
        var projPos = vizPos + projK * projLen * projDir;

        destPos = projPos;

        // step 3: we may have projected ourselves into objects or into a place
        // that is occluded (e.g. by a doorframe), so try and escape these
        // problems

        // step 3.a: first, try to exit vertically. if a surface is blocking the
        // camera, cast up and down along the vertical axis containing the ideal
        // position in the direction of the surface.
        var exitVertDir = Mathf.Sign(vizNormal.y) * Vector3.up;
        var distance = Vector3.Distance(candidate, origin);

        var exitVertLen = 2.0f * distance; // the "diamater" of the curve TODO: is this right?

        didHit = Physiics.BounceCast(
            destPos,
            exitVertDir,
            out m_Hit,
            exitVertLen,
            m_Tuning.CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        // step 3.b: if we didn't exit vertically, we're still in the viz plane.
        // try to exit along it to deal with occluders like doorways by casting
        // from the viz pos to dest pos.
        if (!didHit) {
            var exitPlaneSrc = vizPos;
            var exitPlaneDst = destPos;

            didHit = Physics.Linecast(
                exitPlaneSrc,
                exitPlaneDst,
                out m_Hit,
                m_Tuning.CollisionMask,
                QueryTriggerInteraction.Ignore
            );

            if (didHit) {
                destPos = OffsetHit(m_Hit);
            }
        }

        // step 4: do a final vision cast from the player to make sure the destination
        // point is visible.
        var vizCastEndSrc = origin;
        var vizCastEndDst = destPos;

        didHit = Physics.Linecast(
            vizCastEndSrc,
            vizCastEndDst,
            out m_Hit,
            m_Tuning.CollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (didHit) {
            destPos = OffsetHit(m_Hit);
        }

        return destPos;
    }

    public void SetInvertX(bool value) {
        m_Tuning.IsInvertedX = value;
    }

    public void SetInvertY(bool value) {
        m_Tuning.IsInvertedY = value;
    }

    /// the target's position
    public Vector3 TargetPosition {
        get => m_Destination.position;
    }

    /// the base follow distance
    public float BaseDistance {
        get => m_Tuning.MinRadius;
    }

    /// the minimum follow distance
    public float MinDistance {
        get => m_Tuning.MinRadius * Mathf.Cos(Mathf.Deg2Rad * m_Tuning.FreeLook_MinPitch);
    }

    /// if free look is enabled
    public bool IsFreeLookEnabled {
        get => !m_State.IsTracking;
    }

    /// the hit point adjusted by the contact offset
    Vector3 OffsetHit(RaycastHit hit) {
        return hit.point + m_Tuning.ContactOffset * hit.normal;
    }
}

}