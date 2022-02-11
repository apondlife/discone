using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
    public class CollisionUnityEvent : MonoBehaviour
    {
        [SerializeField] public UnityEvent OnEvent;

        [SerializeField] public bool OnEnter;
        [SerializeField] public bool OnStay;
        [SerializeField] public bool OnExit;

        public bool filterByTag;
        public string tag;

        public bool filterByLayer;
        public LayerMask layerMask;

        private void OnCollisionEnter(Collision other)
        {
            DoTrigger(other, OnEnter);
        }

        private void OnCollisionExit(Collision other)
        {
            DoTrigger(other, OnExit);
        }

        private void OnCollisionStay(Collision other)
        {
            DoTrigger(other, OnStay);
        }

        private void DoTrigger(Collision other, bool ofType)
        {
            if (filterByTag && other.gameObject.tag != tag) return;
            if (filterByLayer && !(layerMask == (layerMask | (1 << other.gameObject.layer)))) return;
            if (ofType) OnEvent.Invoke();
        }
    }
}