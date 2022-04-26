using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MutCommon;

public class RegionSign : MonoBehaviour
{
    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI m_Text;

    [SerializeField]
    internal UIShader m_UIShader;

    [SerializeField]
    internal float dissolveTime = 1f;

     [SerializeField]
    internal float textDuration = 4f;

    [SerializeField]
    internal float letterboxTweenTime = 1f;

     //[SerializeField]
    internal float letterboxDuration;


    // Start is called before the first frame update
    void Start()
    {
        letterboxDuration = textDuration + dissolveTime;

        canvasGroup.alpha = 0;
        m_UIShader.letterboxAmount = 0;
        m_UIShader.dissolveAmount = 1;
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DissolveIn(float k) {
        m_UIShader.dissolveAmount = 1 - k;
    }

    void DissolveOut(float k) {
        m_UIShader.dissolveAmount = k;
    }

    void StartDissolveIn() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(dissolveTime, DissolveIn));
    }

    void StartDissolveOut() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(dissolveTime, DissolveOut));
    }

     void LetterboxIn(float k) {
        m_UIShader.letterboxAmount = k;
    }

    void StartLetterboxIn() {
        Debug.Log("Starting letterbox in...");
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxIn));
    }

    void LetterboxOut(float k) {
        m_UIShader.letterboxAmount = 1 - k;
    }

    void StartLetterboxOut() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxOut));
    }




    public void OnRegionEntered(string regionName) {
        canvasGroup.alpha = 1;
        m_UIShader.letterboxAmount = 0;
        m_UIShader.dissolveAmount = 1;

        m_Text.SetText(regionName);

        // tween in letterbox (and start dissolving when letterbox is done)
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxIn, StartDissolveIn));

        // set dissolve out to start after textDuration
        StartCoroutine(CoroutineHelpers.DoAfterTimeCoroutine(textDuration, StartDissolveOut));
        
        // set letterbox to start after letterboxDuration
        StartCoroutine(CoroutineHelpers.DoAfterTimeCoroutine(letterboxDuration, StartLetterboxOut));
    

    }
}
