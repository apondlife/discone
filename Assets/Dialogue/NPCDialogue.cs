using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour {

  [Header("THE IMPORTANT THING")]
  [Tooltip("the yarn node for the dialogue for this npc")]
  [SerializeField] private string dialogueMessage;

  [Header("references")]
  [Tooltip("the yarn node for the dialogue for this npc")]
  [SerializeField] private GameObject talkable;
  [SerializeField] private Texture characterPreview;

  [Header("dialogue atoms")]
  // TODO: have a bool reference in a child component on the player that stops it during dialogs
  [SerializeField] private BoolReference m_IsDialogueBusy;
  [SerializeField] private StringEvent m_StartDialogue;
  [SerializeField] private VoidEvent m_NextLine;

  [SerializeField] private GameObjectEvent m_StartDialogueWithCharacter;
  [SerializeField] private GameObject Character;


  private const string _dialogueTargetTag = "PlayerDialogueTarget";
  [SerializeField] private bool _playerInTalkRange = false;

  void Start () {
    if (talkable) talkable.SetActive(false);
  }

  // NOTE: this should never run if we're already talking, should check for input before this
  public void TryTalk() {
    if (_playerInTalkRange && !m_IsDialogueBusy.Value) {
        Debug.Log("start dialog " + dialogueMessage);
        m_StartDialogue.Raise(dialogueMessage);
        m_StartDialogueWithCharacter.Raise(Character);
    }
  }

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag(_dialogueTargetTag)) {
      Debug.Log("can talk!!!");
      _playerInTalkRange = true;
      if (talkable) talkable.SetActive(true);
    }
  }

  void OnTriggerExit(Collider other) {
    if (other.CompareTag(_dialogueTargetTag)) {
      _playerInTalkRange = false;
      if (talkable) talkable.SetActive(false);
      // TODO: end dialogue on exit, new atom?
    }
  }
}
