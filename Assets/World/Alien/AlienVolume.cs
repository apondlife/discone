using NaughtyAttributes;
using UnityEngine;
using Soil;
using ThirdPerson;
using UnityAtoms.BaseAtoms;
using Random = UnityEngine.Random;

namespace Discone {

// generates alien spheres in a box volume
sealed class AlienVolume: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the range of number of spheres in this chunk")]
    [SerializeField] IntRange m_Count;

    [Tooltip("the range of sized of the spheres")]
    [SerializeField] MapOutCurve m_SphereSize;

    // -- refs --
    [Header("refs")]
    [Tooltip("the sphere that aliens place around the world")]
    [SerializeField] GameObject m_AlienSphere;

    [Tooltip("the size of the side of the volume (cube)")]
    [SerializeField] FloatReference m_VolumeSize;

    // -- commands --
    [Button("Generate")]
    void Generate() {
        var t = transform;

        // destroy existing spheres
        while (t.childCount > 0) {
            DestroyImmediate(t.GetChild(0).gameObject);
        }

        // generate new spheres
        var count = Random.Range(m_Count.Min, m_Count.Max);
        for (var i = 0; i < count; i++) {
            // randomize position
            var position = new Vector3(
                m_VolumeSize * (Random.value - 0.5f),
                m_VolumeSize * (Random.value - 0.5f),
                m_VolumeSize * (Random.value - 0.5f)
            );

            // randomize scale
            var scale = Vector3.one * m_SphereSize.Evaluate(Random.value);

            // create the sphere
            var sphere = Instantiate(
                m_AlienSphere,
                Vector3.zero,
                Quaternion.identity,
                t
            );

            sphere.transform.localPosition = position;
            sphere.transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected() {
        var color = Color.red;
        color.a = 0.5f;
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, Vector3.one * m_VolumeSize);
    }
}

}