using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityAtoms.BaseAtoms;

/// an online character
class OnlineCharacter: NetworkBehaviour {
    // -- references --
    [Tooltip("a reference to the player character")]
    [SerializeField] GameObjectVariable m_PlayerCharacter;

    // -- NetworkBehaviour --
    public override void OnStartClient() {
        base.OnStartClient();

        // disable camera for other players
        if (!isLocalPlayer) {
            var cam = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
            cam.enabled = false;
            gameObject.name = "PlayerCharacter (other)";

            // TODO: for "rollback" would be nice not to disable this
            var tp = GetComponentInChildren<ThirdPerson.ThirdPerson>();
            tp.enabled = false;

            // TODO: right now we are syncing animation using Network Animator,
            // but might be better to just sync the state and have the model do its job
            var model = GetComponentInChildren<ThirdPerson.CharacterModel>();
            model.enabled = false;
        }
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        // give the local player a special name
        gameObject.name = "PlayerCharacter (local)";

        // replace the offline character with the online one (see: Online.OnCreateCharacter)
        Destroy(m_PlayerCharacter.Value);
        m_PlayerCharacter.Value = gameObject;
    }

}
