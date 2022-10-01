using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
    public class TriggerUnityEvent : MonoBehaviour
    {
        [SerializeField] public UnityEvent OnEvent;

        [SerializeField] public bool OnEnter;
        [SerializeField] public bool OnStay;
        [SerializeField] public bool OnExit;
        [SerializeField] public float StayDuration;

        public bool filterByTag;
        public string filterTag;

        public bool filterByLayer;
        public LayerMask filterLayerMask;

        private void OnTriggerEnter(Collider other)
        {
            DoTrigger(other, OnEnter);
        }

        private void OnTriggerExit(Collider other)
        {
            DoTrigger(other, OnExit);
            currentStayDuration = 0;
            wentOverDuration = false;
        }

        public float currentStayDuration = 0;
        bool wentOverDuration = false;
        private void OnTriggerStay(Collider other)
        {
            if ((filterByTag && other.tag == filterTag) || (filterByLayer && (filterLayerMask == (filterLayerMask | (1 << other.gameObject.layer)))))
            {
                currentStayDuration += Time.deltaTime;
                if (currentStayDuration > StayDuration)
                {
                    DoTrigger(other, OnStay);
                    wentOverDuration = true;
                }
            }
        }

        private void DoTrigger(Collider other, bool ofType)
        {
            if (filterByTag && other.tag != filterTag) return;
            if (filterByLayer && !(filterLayerMask == (filterLayerMask | (1 << other.gameObject.layer)))) return;
            if (ofType) OnEvent.Invoke();
        }
    }
}