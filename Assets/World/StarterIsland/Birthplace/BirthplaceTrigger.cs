using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

sealed class BirthplaceTrigger: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the mechanic's birthplace step to set on enter")]
    [FromList(typeof(MechanicBirthplaceStep))]
    [SerializeField] string m_Step;

    // -- dispatched --
    [Header("dispatched")]
    [Tooltip(".")]
    [SerializeField] StringEvent m_Mechanic_SetBirthplaceStep;

    // -- events --
    void OnTriggerEnter() {
        if (gameObject == null) {
            return;
        }

        Debug.Log($"[help] destroy {m_Step} {transform.position}");
        m_Mechanic_SetBirthplaceStep.Raise(m_Step);
        Destroy(gameObject);
    }
}

}