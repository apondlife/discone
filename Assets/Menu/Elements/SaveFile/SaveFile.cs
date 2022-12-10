using UnityEngine;
using UnityEngine.EventSystems;

namespace Discone.Ui {

[ExecuteAlways]
sealed class SaveFile: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the relative path to the file")]
    [SerializeField] string m_Path; // TODO: this could be an enum

    // -- refs --
    [Header("refs")]
    [Tooltip("the record store")]
    [SerializeField] Store m_Store;

    // -- events --
    public void OnCopyPressed() {
        m_Store.CopyPath(m_Path);
    }

    public void OnDeletePressed() {
        m_Store.Delete(m_Path);
    }
}

}
