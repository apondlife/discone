using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegionSign : MonoBehaviour
{
    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI m_Text;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRegionEntered(string regionName) {
        canvasGroup.alpha = 1;
        m_Text.SetText(regionName);
    }
}
