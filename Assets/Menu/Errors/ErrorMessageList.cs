using UnityEngine;
using UnityAtoms.BaseAtoms;

/// the ui for a list of error messages
public class ErrorMessageList: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the time to fade out a message")]
    [SerializeField] float m_FadeDuration;

    [Tooltip("the time -> alpha curve for a message")]
    [SerializeField] AnimationCurve m_FadeCurve;

    [Tooltip("the template for the error message (replaces {0} with message)")]
    [SerializeField] string m_MessageTemplate = "[ERROR] {0}";

    // -- events --
    [Header("events")]
    [Tooltip("the error stream")]
    [SerializeField] StringEvent m_ErrorEvent;

    // -- refs --
    [Header("refs")]
    [Tooltip("the error message prefab")]
    [SerializeField] TMPro.TMP_Text m_ErrorView;

    // -- props --
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    private void Start() {
        m_Subscriptions
            .Add(m_ErrorEvent, OnError);
    }

    private void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- events --
    private void OnError(string message) {
        // append the error
        var error = Instantiate(m_ErrorView, transform);
        error.text = string.Format(m_MessageTemplate, message);

        // fade out error after duration
        var initialAlpha = error.alpha;
        StartCoroutine(CoroutineHelpers.InterpolateByTime(
            m_FadeDuration,
            (k) => error.alpha = initialAlpha * m_FadeCurve.Evaluate(k),
            ( ) => Destroy(error.gameObject)
        ));
    }

}
