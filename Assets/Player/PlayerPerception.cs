using UnityEngine;

/// the player's sphere of perception
class PlayerPerception: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the collider for perceiving objects")]
    [SerializeField] SphereCollider m_Collider;

    // -- props --
    /// the mask of perceived layers
    LayerMask m_PerceivedLayers;

    // -- lifecycle --
    void Awake() {
        // this object's layer
        var layer = gameObject.layer;

        // detect mask of perceived layers
        var perceived = 0;

        // search for all objects in the surround
        for (int other = 0; other < 32; other++) {
            var name = LayerMask.LayerToName(other);
            if (name == null || name == "") {
                continue;
            }

            if (!Physics.GetIgnoreLayerCollision(layer, other)) {
                perceived |= 1 << other;
            }
        }

        m_PerceivedLayers.value = perceived;

        // find all initially perceived objects
        var t = transform;
        var c = m_Collider;

        var all = Physics.SphereCastAll(
            t.position,
            c.radius,
            Vector3.forward,
            c.radius,
            m_PerceivedLayers.value,
            QueryTriggerInteraction.Collide
        );

        foreach (var collision in all) {
            var target = FindPerceptionTarget(collision.collider);
            if (target != null) {
                Debug.Log($"");
                target.IsPerceived = true;
            }
        }
    }

    // -- queries --
    /// find the nearest perceivable object
    /// TODO: this could use an interface like IPerceptionTarget if necessary
    OnlineCharacter FindPerceptionTarget(Collider other) {
        return other.GetComponentInParent<OnlineCharacter>();
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        var target = FindPerceptionTarget(other);
        if (target != null){
            target.IsPerceived = true;
        }
    }

    void OnTriggerExit(Collider other) {
        var target = FindPerceptionTarget(other);
        if (target != null){
            target.IsPerceived = false;
        }
    }
}
