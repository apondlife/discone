using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MutCommon;
using UnityAtoms.BaseAtoms;

public class RegionSign : MonoBehaviour
{
    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI m_Text;


    [SerializeField]
    internal UIShader m_UIShader;


    [Header("atoms")]
    [SerializeField] FloatVariable m_DissolveAmount;

    [SerializeField] FloatVariable m_LetterboxAmount;


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
        //m_UIShader.letterboxAmount = 0;

        m_LetterboxAmount.Value = 0;
        //m_UIShader.dissolveAmount = 1;

        m_DissolveAmount.Value = 1;
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DissolveIn(float k) {
        //m_UIShader.dissolveAmount = 1 - k;
        m_DissolveAmount.Value = 1 - k;
    }

    void DissolveOut(float k) {
        //m_UIShader.dissolveAmount = k;
        m_DissolveAmount.Value = k;
        
    }

    void StartDissolveIn() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(dissolveTime, DissolveIn));
    }

    void StartDissolveOut() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(dissolveTime, DissolveOut));
    }

     void LetterboxIn(float k) {
        //m_UIShader.letterboxAmount = k;
        m_LetterboxAmount.Value = k;
    }

    void StartLetterboxIn() {
        Debug.Log("Starting letterbox in...");
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxIn));
    }

    void LetterboxOut(float k) {
        //m_UIShader.letterboxAmount = 1 - k;
        m_LetterboxAmount.Value = 1 - k;
    }

    void StartLetterboxOut() {
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxOut));
    }




    public void OnRegionEntered(string regionName) {
        canvasGroup.alpha = 1;
        // m_UIShader.letterboxAmount = 0;
        // m_UIShader.dissolveAmount = 1;
        m_LetterboxAmount.Value = 0;
        m_DissolveAmount.Value = 1;

        m_Text.SetText(regionName);

        // tween in letterbox (and start dissolving when letterbox is done)
        StartCoroutine(CoroutineHelpers.InterpolateByTime(letterboxTweenTime, LetterboxIn, StartDissolveIn));

        // set dissolve out to start after textDuration
        StartCoroutine(CoroutineHelpers.DoAfterTimeCoroutine(textDuration, StartDissolveOut));
        
        // set letterbox to start after letterboxDuration
        StartCoroutine(CoroutineHelpers.DoAfterTimeCoroutine(letterboxDuration, StartLetterboxOut));
    

    }
}
