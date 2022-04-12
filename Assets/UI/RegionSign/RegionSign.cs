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

    private float testK = 0;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
          Debug.Log(testK);
    }

    void TestAction(float k) {
        testK = k;
    }

    public void OnRegionEntered(string regionName) {
        canvasGroup.alpha = 1;
        StartCoroutine(CoroutineHelpers.InterpolateByTime(1f, TestAction));
      
        // funciton that recives a k goes from 0 to 1
        // 3rd param takes that k and does somethign with it
        m_Text.SetText(regionName);
    }
}
