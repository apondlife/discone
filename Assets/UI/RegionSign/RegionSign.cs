using System.Collections;
using UnityEngine;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityAtoms.Discone;
using UnityEngine.Serialization;

namespace Discone {

sealed class RegionSign: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the time it takes for the same region sign to reappear if reentering same previous region")]
    [SerializeField] float m_RepeatCooldown = 60.0f;

    [Tooltip("the time it takes to dissolve the text in/out")]
    [SerializeField] float m_DissolveTime = 1.0f;

    [Tooltip("the time the text is visible")]
    [SerializeField] float m_TextDuration = 4.0f;

    [Tooltip("the time it takes the letterbox to transition in/out")]
    [SerializeField] float m_LetterboxFadeTime = 1.0f;

    // -- state --
    [Header("state")]
    [Tooltip("the text dissolve percent")]
    [SerializeField] FloatVariable m_DissolveAmount;

    [Tooltip("the letterbox transition percent")]
    [SerializeField] FloatVariable m_LetterboxAmount;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when the local player enters a region")]
    [SerializeField] RegionEvent m_RegionEntered;

    // -- refs --
    [Header("refs")]
    [Tooltip("the text canvas")]
    [FormerlySerializedAs("canvasGroup")]
    [SerializeField] CanvasGroup m_CanvasGroup;

    [Tooltip("the region name label")]
    [SerializeField] TextMeshProUGUI m_Text;

    // -- props --
    /// the local player's current region
    Region m_CurrentRegion;

    /// if the region sign is visible
    bool m_IsVisible;

    /// last time at which entered current region
    float m_CurrentRegionEnterTime;

    /// a bag of subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        // hide by default
        m_CanvasGroup.alpha = 0f;
        m_LetterboxAmount.Value = 0f;
        m_DissolveAmount.Value = 0f;
        m_CurrentRegion = null;

        // bind events
        m_Subscriptions
            .Add(m_RegionEntered, OnRegionEntered);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    void DissolveIn(float k) {
        m_DissolveAmount.Value = k;
    }

    void DissolveOut(float k) {
        m_DissolveAmount.Value = 1 - k;
    }

    void LetterboxIn(float k) {
        m_LetterboxAmount.Value = k;
    }

    void LetterboxOut(float k) {
        m_LetterboxAmount.Value = 1 - k;
    }

    // this should do the following:
    // start leterbox, once its done, start text fade
    // once text fade is done, fade letterbox out
    // maybe states could be:
    // for FullState => None => LetterIn => TextIn => TextFull => TextOut => LetterOut => None
    // or (BoxState, TextState): (out, out) => (fadein, out) => (in, fadein) => (in, in) => (in, fadeout) => (fadeout, out) => (out out)
    // TODO: if interrupted, maybe add new texts to a queue and keep letterbox?
    IEnumerator FadeCoroutine() {
        // flag as visible
        m_IsVisible = true;

        // letterbox fade in
        yield return CoroutineHelpers.InterpolateByTime(m_LetterboxFadeTime, LetterboxIn);

        // letterbox in, text fade in
        yield return CoroutineHelpers.InterpolateByTime(m_DissolveTime, DissolveIn);

        // text in
        yield return new WaitForSeconds(m_TextDuration);

        // text fade out
        yield return CoroutineHelpers.InterpolateByTime(m_DissolveTime, DissolveOut);

        // text out, letterbox fade out
        yield return CoroutineHelpers.InterpolateByTime(m_LetterboxFadeTime, LetterboxOut);

        // flag as invisible
        m_IsVisible = false;
    }

    public void OnRegionEntered(Region next) {
        var time = Time.time;
        var prev = m_CurrentRegion;

        var showsRegionSign = (
            // don't show first region sign
            prev != null &&
            // don't show if still in cooldown for current region
            (prev != next || time - m_CurrentRegionEnterTime < m_RepeatCooldown)
        );

        m_CurrentRegion = next;

        // debounce current region enter time
        m_CurrentRegionEnterTime = time;

        if(!showsRegionSign) {
            return;
        }

        Debug.Log(Tag.Region.F($"show sign {prev?.DisplayName} -> {next?.DisplayName}"));

        m_CanvasGroup.alpha = 1f;
        m_Text.SetText(next.DisplayName);

        // only start a new animation if the current one is over
        if (m_IsVisible) {
            return;
        }

        StartCoroutine(FadeCoroutine());
    }
}

}