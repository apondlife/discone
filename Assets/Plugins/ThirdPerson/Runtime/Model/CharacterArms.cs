using UnityEngine;

namespace ThirdPerson {

/// a pair of legs working in unison
class CharacterArms: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the left arm")]
    [SerializeField] Limb m_Left;

    [Tooltip("the right arm")]
    [SerializeField] Limb m_Right;

    // -- refs --
    [Header("refs")]
    [Tooltip("the attached model")]
    [SerializeField] Transform m_Model;

    // -- props --
    /// the character's dependency container
    CharacterContainer c;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    void Update() {
        MoveArm(m_Left);
        MoveArm(m_Right);
    }

    // -- commands --
    void MoveArm(Limb arm) {
        if (c.State.Curr.IsIdle) {
            return;
        }

        // if arm is moving, don't change target
        if (!arm.State.IsFree && !arm.State.IsHeld) {
            return;
        }

        var currDir = arm.RootPos - arm.GoalPos;
        var fwd = c.State.Curr.Velocity.normalized;
        if (fwd == Vector3.zero) {
            fwd = c.State.Curr.Forward;
        }

        // get the signed distance of the held arm
        var currDist = Vector3.Dot(currDir, fwd);

        // if far enough away, look for an anchor position
        // AAA: this dist is projected into the surface
        // should probably just be doing the whole move method to prevent clipping
        var maxStride = Mathf.Max(arm.Tuning.MaxLength.Max, arm.Tuning.MaxLength.Min);
        maxStride = Mathf.Max(maxStride, maxStride * arm.Tuning.MaxLength_CrossScale);

        var maxDist = Mathf.Max(
            arm.InitialLen,
            maxStride / Vector3.Cross(currDir.normalized, arm.SearchDir).magnitude
        );

        if (currDist < maxDist) {
            return;
        }

        var currProjSearch = Vector3.Project(currDir, arm.SearchDir);
        var castDir = (currDir - 2f * currProjSearch).normalized;
        var castSrc = arm.RootPos;

        var castLen = arm.InitialLen + arm.Tuning.SearchRange_OnSurface / Vector3.Dot(castDir, arm.SearchDir);

        DebugDraw.Push(arm.Goal.Debug_Name("phantom-cast"), castSrc, castDir * castLen, new DebugDraw.Config(arm.Goal.Debug_Color(), count: 1));

        var didHit = Physics.Raycast(
            castSrc,
            castDir,
            out var hit,
            castLen,
            arm.Tuning.CastMask,
            QueryTriggerInteraction.Ignore
        );

        if (!didHit) {
            return;
        }

        arm.State.Anchor = new LimbAnchor(
            rootPos: arm.RootPos,
            goalPos: hit.point
        );

        arm.Move();
    }
}

}