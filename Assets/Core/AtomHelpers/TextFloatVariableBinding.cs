using UnityEngine;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class TextFloatVariableBinding : MonoBehaviour
    {
        [SerializeField] private FloatReference m_Value;
        [SerializeField] private string m_Template = "{0}";

        private TMPro.TMP_Text m_TextComponent;

        private void OnValidate() {
            m_TextComponent = GetComponent<TMPro.TMP_Text>();
            Render(m_Value);
        }

        private void Start()
        {
            m_TextComponent = GetComponent<TMPro.TMP_Text>();
            m_Value.GetChanged()?.Register(Render);
            Render(m_Value);
        }

        private void Render(float t) {
            m_TextComponent.text = string.Format(m_Template, t);
        }
    }
}