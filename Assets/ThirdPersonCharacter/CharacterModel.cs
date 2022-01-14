using UnityEngine;

// needs a reference to ThirdPersonCharacter
public class CharacterModel: MonoBehaviour {
    // -- fields --
    [SerializeField] private CharacterState state;
    [SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var moveSpeed = state.PlanarVelocity.magnitude / 10;
        animator.SetFloat("MoveSpeed", moveSpeed);
        

        var airborne = !state.IsGrounded;
        animator.SetBool("Airborne", airborne);
    }
}
