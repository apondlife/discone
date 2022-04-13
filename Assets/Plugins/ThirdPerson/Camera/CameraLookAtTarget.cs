using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPerson {

// a position below the target, on the ground if possible
public class CameraLookAtTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the max distance from the character to cast for the ground")]
    [SerializeField] private float m_MaxDistance;

    [Tooltip("the layer mask for the ground")]
    [SerializeField] private LayerMask m_GroundLayers;

    // -- refs --
    [Header("refs")]
    [Tooltip("the target transform")]
    [SerializeField] private Transform m_Target;

    // -- lifecycle --
    void Update() {
        // move target to ground position underneath character
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, m_MaxDistance, m_GroundLayers)) {
            m_Target.position = hit.point;
        }
        // snap to the lowest possible position
        else {
            m_Target.position = transform.position + Vector3.down * m_MaxDistance;
        }
    }

    // -- queries --
    public float PercentExtended {
        get => Mathf.Clamp01(Vector3.Distance(transform.position, m_Target.position)/m_MaxDistance);
    }

    // -- gizmos --
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * m_MaxDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, m_Target.position);
        Gizmos.DrawSphere(m_Target.position, 0.3f);
    }
}

}