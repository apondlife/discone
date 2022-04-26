using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineRecomposer))]
public class MouseRecomposer : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_MouseDelta;

    [SerializeField]
    private float m_Sensitivity;

    [SerializeField]
    private float m_MaxTilt;

    [SerializeField]
    private float m_MaxYaw;

    private CinemachineRecomposer recomposer;

    private void OnValidate() {
        m_MaxTilt = Mathf.Abs(m_MaxTilt);
        m_MaxYaw = Mathf.Abs(m_MaxYaw);
        m_Sensitivity = Mathf.Max(m_Sensitivity, 0.01f);
    }

    // Start is called before the first frame update
    void Awake()
    {
        recomposer = GetComponent<CinemachineRecomposer>();
    }

    private void OnEnable() {
        recomposer.m_Tilt = 0;
        recomposer.m_Pan = 0;
    }

    // Update is called once per frame
    void Update()
    {
        var delta = m_MouseDelta.action.ReadValue<Vector2>() * m_Sensitivity * Time.deltaTime;

        // subtract cause y is inverted
        recomposer.m_Tilt = Mathf.Clamp(recomposer.m_Tilt - delta.y, -m_MaxTilt, m_MaxTilt);
        recomposer.m_Pan = Mathf.Clamp(recomposer.m_Pan + delta.x, -m_MaxYaw, m_MaxYaw);
    }
}
