using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TalkableIndicator : MonoBehaviour
{
    [Header("Fade")]
    [Tooltip("how much time it takes for the indicator to appear")]
    [SerializeField] private float m_FadeIn;

    [Tooltip("how much time it takes for the indicator to disappear")]
    [SerializeField] private float m_FadeOut;

    [Header("Oscilation")]
    [Tooltip("how much time it takes for the indicator to loop")]
    [SerializeField] private float m_Period;

    [Tooltip("how much up the indicator oscilates")]
    [SerializeField] private float m_HeightDelta;

    private bool m_IsVisible = false;

    private Vector3 startPosition;
    private TMP_Text text;
    private float time;
    private float hideTime = 0;
    private float fontSize;

    // Start is called before the first frame update
    private void Awake() {
        text = GetComponent<TMP_Text>();
        startPosition = transform.localPosition;
        fontSize = text.fontSize;
        SetFade(0);
    }

    public void Show() {
        if(m_IsVisible) return;
        time = 0;
        m_IsVisible = true;
    }

    public void Hide() {
        if(!m_IsVisible) return;

        hideTime = time;
        m_IsVisible = false;
    }

    void SetFade(float alpha) {
        text.fontSize = alpha * fontSize;
    }

    // Update is called once per frame
    void Update()
    {
        // billboard (always face the camera)
        var r = transform.rotation.eulerAngles;
        r.y = Camera.main.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(r.x, r.y, r.z);

        if(m_IsVisible) {
            if(time < m_FadeIn) {
                var k = time / m_FadeIn;
                SetFade(k * k);
            } else {
                SetFade(1);
            }
        } else {
            var hide = time - hideTime;
            if(hide < m_FadeOut) {
                var k = hide/m_FadeOut;
                SetFade(1 - k*k);
            } else {
                SetFade(0);
            }
        }

        // make it so the bo
        var offset = m_HeightDelta/2 + m_HeightDelta / 2 * Mathf.Sin(2 * Mathf.PI / m_Period * time);
        transform.localPosition = startPosition + Vector3.up * offset;
        time += Time.deltaTime;
    }
}

