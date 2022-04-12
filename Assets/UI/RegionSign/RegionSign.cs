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


    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.alpha = 0;
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

    public void OnRegionEntered(string regionName) {
        canvasGroup.alpha = 1;
        m_UIShader.dissolveAmount = 1;
        StartCoroutine(CoroutineHelpers.InterpolateByTime(1f, DissolveIn));
      
        // funciton that recives a k goes from 0 to 1
        // 3rd param takes that k and does somethign with it
        m_Text.SetText(regionName);
    }
}
