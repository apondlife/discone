using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class FPSCounter : MonoBehaviour
{
    [Header("config")]
    [Tooltip("the Period to log the fps")]
    [SerializeField] private float m_LogPeriod;

    [Header("refs")]
    [Tooltip("the last updated fps")]
    [SerializeField] private FloatVariable m_FPS;

    private float m_Timer;
    private int m_Frames;


    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.deltaTime;
        m_Frames ++;

        if(m_Timer > m_LogPeriod) {
            var fps = (float)m_Frames / m_Timer;

            m_FPS?.SetValue(fps);
            Debug.Log($"[FPS] Total frames: {m_Frames}. FPS: {fps}");

            m_Timer = 0;
            m_Frames = 0;
        }
    }
}
