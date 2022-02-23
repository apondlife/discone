using UnityEngine;
namespace ThirdPerson {

[System.Serializable]
public abstract class InputProvider : MonoBehaviour {
    public virtual void Init() {

    }

    public abstract Vector2 Move {
        get;
    }

    public abstract bool IsJumpPressed {
        get;
    }
}

}