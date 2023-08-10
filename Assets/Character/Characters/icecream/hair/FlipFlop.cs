using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipFlop : MonoBehaviour
{
    [SerializeField] float m_MaxSpeed;
    [SerializeField] float m_Period;


    Rigidbody m_Rigidbody;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        var sign = Mathf.Sin(2 * Mathf.PI * Time.time / m_Period);
        var velocity = transform.forward * m_MaxSpeed * sign;
        m_Rigidbody.transform.localPosition += velocity * Time.deltaTime;
    }
}
