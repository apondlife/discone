using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NeueContinueAnimator : MonoBehaviour
{
    // this class animates some elipssis

    TextMeshProUGUI m_Text = null;

    int numVisible = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        m_Text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(AnimateElipsis());
        
    }

    IEnumerator AnimateElipsis() {

        //lineText.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_Text.textInfo;
        int characterCount = 3; // m_Text.textInfo.characterCount;
        numVisible = characterCount;

        while (true) {
            numVisible = (numVisible + 1) % characterCount;
            m_Text.maxVisibleCharacters = numVisible + 1;
            m_Text.ForceMeshUpdate();

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
